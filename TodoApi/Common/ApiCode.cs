namespace TodoApi.Common;

/// <summary>业务状态码。</summary>
public static class ApiCode
{
    public const int Success = 0;
    public const int ValidationError = 1001;
    public const int BusinessError = 1002;
    public const int NotFound = 1003;
    public const int Conflict = 1004;
    public const int ServerError = 5000;
}
