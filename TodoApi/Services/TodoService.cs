using AutoMapper;
using TodoApi.Common;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services;

/// <summary>待办事项业务逻辑。</summary>
public class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TodoService> _logger;

    public TodoService(
        ITodoRepository todoRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<TodoService> logger)
    {
        _todoRepository = todoRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<TodoResponse>> GetListAsync(TodoQuery query, CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        var source = _todoRepository.BuildQuery(query);
        var totalCount = await _todoRepository.CountAsync(source, cancellationToken);
        var sorted = _todoRepository.ApplySort(source, query.SortBy, query.Desc);
        var items = await _todoRepository.ListPagedAsync(sorted, page, pageSize, cancellationToken);

        _logger.LogInformation(
            "查询待办列表 | Keyword={Keyword} IsCompleted={IsCompleted} Priority={Priority} CategoryId={CategoryId} " +
            "SortBy={SortBy} Desc={Desc} Page={Page} PageSize={PageSize} Total={Total}",
            query.Keyword, query.IsCompleted, query.Priority, query.CategoryId,
            query.SortBy, query.Desc, page, pageSize, totalCount);

        return new PagedResult<TodoResponse>
        {
            Items = _mapper.Map<List<TodoResponse>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TodoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _todoRepository.GetWithCategoryAsync(id, cancellationToken);
        if (item is null)
        {
            throw NotFoundException.For("待办事项", id);
        }

        return _mapper.Map<TodoResponse>(item);
    }

    public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var item = _mapper.Map<TodoItem>(request);
        item.IsCompleted = false;
        item.CreatedAt = DateTime.Now;

        _todoRepository.Add(item);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("新增待办 | Id={Id} Title={Title} CategoryId={CategoryId}",
            item.Id, item.Title, item.CategoryId);

        var created = await _todoRepository.GetWithCategoryAsync(item.Id, cancellationToken);
        return _mapper.Map<TodoResponse>(created ?? item);
    }

    public async Task<TodoResponse> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _todoRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw NotFoundException.For("待办事项", id);
        }

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        _mapper.Map(request, item);
        item.UpdatedAt = DateTime.Now;

        await _todoRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("整体更新待办 | Id={Id} Title={Title} IsCompleted={IsCompleted}",
            item.Id, item.Title, item.IsCompleted);

        var updated = await _todoRepository.GetWithCategoryAsync(id, cancellationToken);
        return _mapper.Map<TodoResponse>(updated ?? item);
    }

    public async Task<TodoResponse> PatchAsync(int id, TodoPatchRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _todoRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw NotFoundException.For("待办事项", id);
        }

        if (request.CategoryId.HasValue)
        {
            await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        }

        // 只改提交的字段
        if (request.Title is not null)
        {
            item.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            item.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.IsCompleted.HasValue)
        {
            item.IsCompleted = request.IsCompleted.Value;
        }

        if (request.Priority.HasValue)
        {
            item.Priority = request.Priority.Value;
        }

        if (request.DueDate.HasValue)
        {
            item.DueDate = request.DueDate.Value;
        }

        if (request.CategoryId.HasValue)
        {
            item.CategoryId = request.CategoryId.Value;
        }

        item.UpdatedAt = DateTime.Now;
        await _todoRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("局部更新待办 | Id={Id} IsCompleted={IsCompleted} Priority={Priority}",
            item.Id, item.IsCompleted, item.Priority);

        var updated = await _todoRepository.GetWithCategoryAsync(id, cancellationToken);
        return _mapper.Map<TodoResponse>(updated ?? item);
    }

    public async Task<TodoResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // 删除要用带跟踪的查询，GetWithCategoryAsync 是无跟踪的
        var item = await _todoRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw NotFoundException.For("待办事项", id);
        }

        _todoRepository.Remove(item);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("删除待办 | Id={Id} Title={Title}", item.Id, item.Title);

        return _mapper.Map<TodoResponse>(item);
    }

    public async Task<int> ClearCompletedAsync(CancellationToken cancellationToken = default)
    {
        var completed = await _todoRepository.GetCompletedAsync(cancellationToken);
        if (completed.Count == 0)
        {
            return 0;
        }

        _todoRepository.RemoveRange(completed);
        await _todoRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("清理已完成待办 | Removed={Removed}", completed.Count);

        return completed.Count;
    }

    public async Task<TodoStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _todoRepository.GetAllAsync(cancellationToken);
        var today = DateTime.Today;

        var total = all.Count;
        var completed = all.Count(t => t.IsCompleted);

        var countByCategory = await _todoRepository.CountByCategoryAsync(cancellationToken);
        var categories = await _categoryRepository.GetAllWithCountAsync(cancellationToken);

        var stats = new TodoStatsResponse
        {
            Total = total,
            Completed = completed,
            Active = total - completed,
            CompletionRate = total == 0 ? 0 : Math.Round(completed * 100.0 / total, 2),
            HighPriorityActive = all.Count(t => !t.IsCompleted && t.Priority == Priority.High),
            Overdue = all.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date < today),
            ByPriority = new Dictionary<string, int>
            {
                ["low"] = all.Count(t => t.Priority == Priority.Low),
                ["medium"] = all.Count(t => t.Priority == Priority.Medium),
                ["high"] = all.Count(t => t.Priority == Priority.High)
            },
            ByCategory = categories.ToDictionary(
                c => c.Name,
                c => countByCategory.TryGetValue(c.Id, out var count) ? count : 0)
        };

        _logger.LogInformation("统计待办 | Total={Total} Completed={Completed} Active={Active} Overdue={Overdue}",
            stats.Total, stats.Completed, stats.Active, stats.Overdue);

        return stats;
    }

    private async Task EnsureCategoryExistsAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is null)
        {
            return;
        }

        if (!await _categoryRepository.ExistsAsync(categoryId.Value, cancellationToken))
        {
            _logger.LogWarning("指定分类不存在 | CategoryId={CategoryId}", categoryId);
            throw new ValidationAppException($"分类（Id={categoryId}）不存在");
        }
    }
}
