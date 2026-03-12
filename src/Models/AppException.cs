namespace OneFlowApis.Models;

public sealed class AppException : Exception
{
    public AppException(int statusCode, string message, object? details = null) : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public object? Details { get; }
}
