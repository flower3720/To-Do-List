namespace TodoApi.Common;

/// <summary>统一返回格式（无数据体，用于异常响应）。</summary>
public class ApiResult
{
    public int Code { get; set; }

    public string Message { get; set; } = string.Empty;

    public object? Data { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string? TraceId { get; set; }

    public ApiResult()
    {
    }

    public ApiResult(int code, string message, object? data = null, string? traceId = null)
    {
        Code = code;
        Message = message;
        Data = data;
        TraceId = traceId;
    }

    public static ApiResult Fail(int code, string message)
        => new(code, message);
}

/// <summary>统一返回格式，Code 为 0 表示成功。</summary>
public class ApiResult<T>
{
    public int Code { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string? TraceId { get; set; }

    public ApiResult()
    {
    }

    public ApiResult(int code, string message, T? data = default, string? traceId = null)
    {
        Code = code;
        Message = message;
        Data = data;
        TraceId = traceId;
    }

    public static ApiResult<T> Ok(T data, string message = "success")
        => new(ApiCode.Success, message, data);

    public static ApiResult<T> Ok(string message = "success")
        => new(ApiCode.Success, message);

    public static ApiResult<T> Fail(int code, string message)
        => new(code, message);
}
