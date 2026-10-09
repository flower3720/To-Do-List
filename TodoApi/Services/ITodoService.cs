using TodoApi.Common;
using TodoApi.Dtos;

namespace TodoApi.Services;

/// <summary>待办事项业务逻辑接口。</summary>
public interface ITodoService
{
    Task<PagedResult<TodoResponse>> GetListAsync(TodoQuery query, CancellationToken cancellationToken = default);

    Task<TodoResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken cancellationToken = default);

    Task<TodoResponse> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken cancellationToken = default);

    Task<TodoResponse> PatchAsync(int id, TodoPatchRequest request, CancellationToken cancellationToken = default);

    Task<TodoResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> ClearCompletedAsync(CancellationToken cancellationToken = default);

    Task<TodoStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default);
}
