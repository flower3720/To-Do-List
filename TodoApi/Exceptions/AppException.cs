using TodoApi.Common;

namespace TodoApi.Exceptions;

/// <summary>业务异常基类，由全局异常中间件统一转换成 ApiResult。</summary>
public abstract class AppException : Exception
{
    public int Code { get; }

    public int StatusCode { get; }

    protected AppException(string message, int code, int statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

/// <summary>资源不存在。</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, ApiCode.NotFound, StatusCodes.Status404NotFound)
    {
    }

    public static NotFoundException For(string resource, int id)
        => new($"{resource}（Id={id}）不存在");
}

/// <summary>资源冲突。</summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, ApiCode.Conflict, StatusCodes.Status409Conflict)
    {
    }
}

/// <summary>参数不合法。</summary>
public class ValidationAppException : AppException
{
    public ValidationAppException(string message)
        : base(message, ApiCode.ValidationError, StatusCodes.Status400BadRequest)
    {
    }
}

/// <summary>业务规则不满足。</summary>
public class BusinessException : AppException
{
    public BusinessException(string message)
        : base(message, ApiCode.BusinessError, StatusCodes.Status400BadRequest)
    {
    }
}
