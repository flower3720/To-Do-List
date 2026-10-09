using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

/// <summary>待办分类实体，与 TodoItem 是一对多关系。</summary>
public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "分类名称长度必须在 1~50 个字符之间")]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
}
