using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>分类仓储接口。</summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllWithCountAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>查询用于修改的分类，带变更跟踪。</summary>
    Task<Category?> GetForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> HasTodoItemsAsync(int categoryId, CancellationToken cancellationToken = default);
}

/// <summary>分类仓储实现。</summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly TodoDbContext _db;

    public CategoryRepository(TodoDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
        => await Query()
            .Include(c => c.TodoItems)            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default)
        => await Query().Include(c => c.TodoItems).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    // 写操作必须用带跟踪的查询：游离实体改了之后 SaveChanges 不会生成 UPDATE
    public async Task<Category?> GetForUpdateAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        => await _db.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), cancellationToken);

    public async Task<bool> HasTodoItemsAsync(int categoryId, CancellationToken cancellationToken = default)
        => await _db.TodoItems
            .AsNoTracking()
            .AnyAsync(t => t.CategoryId == categoryId, cancellationToken);
}
