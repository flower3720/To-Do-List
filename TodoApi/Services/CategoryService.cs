using AutoMapper;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services;

/// <summary>分类业务逻辑。</summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllWithCountAsync(cancellationToken);

        return _mapper.Map<List<CategoryResponse>>(categories);
    }

    public async Task<CategoryResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetWithItemsAsync(id, cancellationToken);
        if (category is null)
        {
            throw NotFoundException.For("分类", id);
        }

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await _categoryRepository.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException($"分类名称“{name}”已存在");
        }

        var category = new Category
        {
            Name = name,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.Now
        };

        _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("新增分类 | Id={Id} Name={Name}", category.Id, category.Name);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetForUpdateAsync(id, cancellationToken);
        if (category is null)
        {
            throw NotFoundException.For("分类", id);
        }

        var name = request.Name.Trim();
        if (await _categoryRepository.NameExistsAsync(name, id, cancellationToken))
        {
            throw new ConflictException($"分类名称“{name}”已存在");
        }

        category.Name = name;
        category.SortOrder = request.SortOrder;

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("更新分类 | Id={Id} Name={Name}", category.Id, category.Name);

        var updated = await _categoryRepository.GetWithItemsAsync(id, cancellationToken);
        return _mapper.Map<CategoryResponse>(updated ?? category);
    }

    public async Task<CategoryResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetForUpdateAsync(id, cancellationToken);
        if (category is null)
        {
            throw NotFoundException.For("分类", id);
        }

        if (await _categoryRepository.HasTodoItemsAsync(id, cancellationToken))
        {
            throw new ConflictException($"分类“{category.Name}”下仍有待办事项，请先移除或转移后再删除");
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("删除分类 | Id={Id} Name={Name}", category.Id, category.Name);

        return _mapper.Map<CategoryResponse>(category);
    }
}
