using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>待办事项仓储接口。</summary>
public interface ITodoRepository : IRepository<TodoItem>
{
    /// <summary>按关键字、状态、优先级、分类组合筛选。</summary>
    IQueryable<TodoItem> BuildQuery(TodoQuery query);

    IQueryable<TodoItem> ApplySort(IQueryable<TodoItem> source, string? sortBy, bool desc);

    Task<TodoItem?> GetWithCategoryAsync(int id, CancellationToken cancellationToken = default);

    Task<List<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default);

    Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> CountByCategoryAsync(CancellationToken cancellationToken = default);
}

/// <summary>待办事项仓储实现。</summary>
public class TodoRepository : Repository<TodoItem>, ITodoRepository
{
    public TodoRepository(TodoDbContext db) : base(db)
    {
    }

    public IQueryable<TodoItem> BuildQuery(TodoQuery query)
    {
        IQueryable<TodoItem> items = Query().Include(t => t.Category);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            items = items.Where(t =>
                t.Title.Contains(keyword) ||
                (t.Description != null && t.Description.Contains(keyword)));
        }

        if (query.IsCompleted.HasValue)
        {
            items = items.Where(t => t.IsCompleted == query.IsCompleted.Value);
        }

        if (query.Priority.HasValue)
        {
            items = items.Where(t => t.Priority == query.Priority.Value);
        }

        if (query.CategoryId.HasValue)
        {
            items = items.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        return items;
    }

    public IQueryable<TodoItem> ApplySort(IQueryable<TodoItem> source, string? sortBy, bool desc)
    {
        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "duedate" => desc ? source.OrderByDescending(t => t.DueDate) : source.OrderBy(t => t.DueDate),
            "priority" => desc
                ? source.OrderByDescending(t => t.Priority).ThenByDescending(t => t.Id)
                : source.OrderBy(t => t.Priority).ThenBy(t => t.Id),
            "title" => desc ? source.OrderByDescending(t => t.Title) : source.OrderBy(t => t.Title),
            "category" => desc
                ? source.OrderByDescending(t => t.Category!.Name).ThenByDescending(t => t.Id)
                : source.OrderBy(t => t.Category!.Name).ThenBy(t => t.Id),
            _ => desc
                ? source.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id)
                : source.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id)
        };
    }

    public async Task<TodoItem?> GetWithCategoryAsync(int id, CancellationToken cancellationToken = default)
        => await Query().Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<List<TodoItem>> GetCompletedAsync(CancellationToken cancellationToken = default)
        => await Query(asNoTracking: false).Where(t => t.IsCompleted).ToListAsync(cancellationToken);

    public async Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Query().ToListAsync(cancellationToken);

    public async Task<Dictionary<int, int>> CountByCategoryAsync(CancellationToken cancellationToken = default)
        => await Query()
            .Where(t => t.CategoryId != null)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
}
