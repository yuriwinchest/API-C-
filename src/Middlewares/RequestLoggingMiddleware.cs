using System.Diagnostics;

namespace OneFlowApis.Middlewares;

public sealed class RequestLoggingMiddleware
{
    private const string CorrelationHeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Response.Headers[CorrelationHeaderName] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["correlationId"] = correlationId,
            ["method"] = context.Request.Method,
            ["path"] = context.Request.Path.ToString()
        });

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Requisicao concluida com status {StatusCode} em {ElapsedMs} ms.",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeaderName].ToString().Trim();
        return string.IsNullOrWhiteSpace(correlationId) ? context.TraceIdentifier : correlationId;
    }
}
