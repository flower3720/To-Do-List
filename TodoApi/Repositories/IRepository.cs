using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;

namespace TodoApi.Repositories;

/// <summary>泛型仓储接口，封装通用的增删改查与分页。</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>获取查询对象，asNoTracking 控制是否跟踪实体。</summary>
    IQueryable<T> Query(bool asNoTracking = true);

    Task<int> CountAsync(IQueryable<T> source, CancellationToken cancellationToken = default);

    Task<List<T>> ListPagedAsync(IQueryable<T> source, int page, int pageSize, CancellationToken cancellationToken = default);

    void Add(T entity);

    void AddRange(IEnumerable<T> entities);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
