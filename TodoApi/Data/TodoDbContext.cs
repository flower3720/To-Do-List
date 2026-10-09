using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

/// <summary>数据库上下文。</summary>
public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Description)
                  .HasMaxLength(500);

            entity.Property(e => e.Priority)
                  .HasConversion<int>();

            entity.HasIndex(e => e.IsCompleted)
                  .HasDatabaseName("IX_TodoItems_IsCompleted");

            entity.HasIndex(e => e.Priority)
                  .HasDatabaseName("IX_TodoItems_Priority");

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.TodoItems)
                  .HasForeignKey(e => e.CategoryId)
                  .HasConstraintName("FK_TodoItems_Categories_CategoryId")
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.CategoryId, e.IsCompleted })
                  .HasDatabaseName("IX_TodoItems_CategoryId_IsCompleted");

            entity.HasData(
                new TodoItem
                {
                    Id = 1,
                    Title = "完成《Web 应用开发》课程设计报告",
                    Description = "包含需求分析、系统设计、接口说明与测试记录。",
                    IsCompleted = false,
                    Priority = Priority.High,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 9, 0, 0),
                    DueDate = new DateTime(2026, 1, 20, 0, 0, 0)
                },
                new TodoItem
                {
                    Id = 2,
                    Title = "复习数据结构期末考点",
                    Description = "重点：二叉树遍历、图的 Dijkstra 算法。",
                    IsCompleted = false,
                    Priority = Priority.High,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 1, 2, 10, 30, 0),
                    DueDate = new DateTime(2026, 1, 25, 0, 0, 0)
                },
                new TodoItem
                {
                    Id = 3,
                    Title = "提交实验三源代码",
                    Description = "打包上传到课程平台。",
                    IsCompleted = true,
                    Priority = Priority.Medium,
                    CategoryId = 1,
                    CreatedAt = new DateTime(2026, 1, 3, 14, 0, 0),
                    UpdatedAt = new DateTime(2026, 1, 5, 20, 0, 0)
                },
                new TodoItem
                {
                    Id = 4,
                    Title = "预约图书馆研讨室",
                    Description = null,
                    IsCompleted = false,
                    Priority = Priority.Low,
                    CategoryId = 2,
                    CreatedAt = new DateTime(2026, 1, 4, 8, 15, 0),
                    DueDate = new DateTime(2026, 1, 28, 0, 0, 0)
                });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("IX_Categories_Name");

            entity.HasData(
                new Category { Id = 1, Name = "课程学习", SortOrder = 1, CreatedAt = new DateTime(2026, 1, 1, 8, 0, 0) },
                new Category { Id = 2, Name = "校园生活", SortOrder = 2, CreatedAt = new DateTime(2026, 1, 1, 8, 5, 0) },
                new Category { Id = 3, Name = "实习求职", SortOrder = 3, CreatedAt = new DateTime(2026, 1, 1, 8, 10, 0) });
        });
    }
}
