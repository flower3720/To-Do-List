using TodoApi.Common;
using TodoApi.Exceptions;

namespace TodoApi.Middleware;

/// <summary>全局异常处理，把异常统一转成 ApiResult 返回。</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex,
                "业务异常 | {Method} {Path} | Code={Code} | Message={Message}",
                context.Request.Method, context.Request.Path, ex.Code, ex.Message);

            await WriteAsync(context, ex.StatusCode, ApiResult.Fail(ex.Code, ex.Message));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("请求被客户端取消 | {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "未处理异常 | {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                ApiResult.Fail(ApiCode.ServerError, "服务器内部错误，请稍后重试"));
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, ApiResult body)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        body.TraceId = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(body);
    }
}
