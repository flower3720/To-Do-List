using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

/// <summary>新增或更新分类的请求体。</summary>
public class CategoryRequest
{
    [Required(ErrorMessage = "分类名称不能为空")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "分类名称长度必须在 1~50 个字符之间")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 9999, ErrorMessage = "排序值必须在 0~9999 之间")]
    public int SortOrder { get; set; }
}

/// <summary>分类响应数据。</summary>
public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>该分类下的待办数量。</summary>
    public int TodoCount { get; set; }
}
