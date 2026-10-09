using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

/// <summary>待办事项实体。</summary>
public class TodoItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "标题不能为空")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "标题长度必须在 1~100 个字符之间")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述最长 500 个字符")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    /// <summary>优先级：0=低，1=中，2=高。</summary>
    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>所属分类 Id，为空表示未分类。</summary>
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}

/// <summary>优先级。</summary>
public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2
}
