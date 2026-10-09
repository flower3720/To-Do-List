using TodoApi.Dtos;

namespace TodoApi.Services;

/// <summary>分类业务逻辑接口。</summary>
public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CategoryResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default);

    Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);

    Task<CategoryResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
