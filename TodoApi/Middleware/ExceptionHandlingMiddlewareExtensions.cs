namespace TodoApi.Middleware;

public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>注册全局异常处理。放在管道最前面。</summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
