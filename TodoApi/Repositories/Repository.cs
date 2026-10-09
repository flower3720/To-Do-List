using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Repositories;

/// <summary>泛型仓储实现，提供所有实体共用的数据访问。</summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly TodoDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(TodoDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object[] { id }, cancellationToken);

    public IQueryable<T> Query(bool asNoTracking = true)
        => asNoTracking ? Set.AsNoTracking() : Set.AsQueryable();

    public async Task<int> CountAsync(IQueryable<T> source, CancellationToken cancellationToken = default)
        => await source.CountAsync(cancellationToken);

    public async Task<List<T>> ListPagedAsync(IQueryable<T> source, int page, int pageSize, CancellationToken cancellationToken = default)
        => await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public void Add(T entity) => Set.Add(entity);

    public void AddRange(IEnumerable<T> entities) => Set.AddRange(entities);

    public void Remove(T entity) => Set.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => Set.RemoveRange(entities);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await Db.SaveChangesAsync(cancellationToken);

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object[] { id }, cancellationToken) is not null;
}
