namespace TodoApi.Common;

/// <summary>分页参数处理。</summary>
public static class PagingHelper
{
    /// <summary>每页最大条数。</summary>
    public const int MaxPageSize = 100;

    /// <summary>默认每页条数。</summary>
    public const int DefaultPageSize = 10;

    /// <summary>修正页码与每页条数，避免客户端传入 0 或超大值。</summary>
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

        return (safePage, safePageSize);
    }
}
