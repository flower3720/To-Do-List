using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.Dtos;

/// <summary>新增待办的请求体。</summary>
public class TodoCreateRequest
{
    [Required(ErrorMessage = "标题不能为空")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "标题长度必须在 1~100 个字符之间")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述最长 500 个字符")]
    public string? Description { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "分类 Id 必须为正整数")]
    public int? CategoryId { get; set; }
}

/// <summary>整体更新待办的请求体。</summary>
public class TodoUpdateRequest
{
    [Required(ErrorMessage = "标题不能为空")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "标题长度必须在 1~100 个字符之间")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述最长 500 个字符")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "分类 Id 必须为正整数")]
    public int? CategoryId { get; set; }
}

/// <summary>局部更新请求体，null 表示不修改该字段。</summary>
public class TodoPatchRequest
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "标题长度必须在 1~100 个字符之间")]
    public string? Title { get; set; }

    [StringLength(500, ErrorMessage = "描述最长 500 个字符")]
    public string? Description { get; set; }

    public bool? IsCompleted { get; set; }

    public Priority? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "分类 Id 必须为正整数")]
    public int? CategoryId { get; set; }
}

/// <summary>列表查询参数。</summary>
public class TodoQuery
{
    public string? Keyword { get; set; }

    public bool? IsCompleted { get; set; }

    public Priority? Priority { get; set; }

    public int? CategoryId { get; set; }

    /// <summary>排序字段：createdAt、dueDate、priority、title、category。</summary>
    public string? SortBy { get; set; }

    public bool Desc { get; set; } = false;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}

/// <summary>待办事项响应数据。</summary>
public class TodoResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public int Priority { get; set; }
    public string PriorityText { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsOverdue { get; set; }
}

/// <summary>统计信息。</summary>
public class TodoStatsResponse
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Active { get; set; }
    public double CompletionRate { get; set; }
    public int HighPriorityActive { get; set; }
    public int Overdue { get; set; }

    /// <summary>各优先级数量，键为 low / medium / high。</summary>
    public Dictionary<string, int> ByPriority { get; set; } = new();

    /// <summary>各分类下的待办数量，键为分类名。</summary>
    public Dictionary<string, int> ByCategory { get; set; } = new();
}
