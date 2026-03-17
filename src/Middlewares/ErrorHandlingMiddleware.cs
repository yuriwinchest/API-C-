using System.Text.Json;
using Microsoft.Extensions.Logging;
using OneFlowApis.Models;

namespace OneFlowApis.Middlewares;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        try
        {
            await _next(context);
        }
        catch (AppException exception)
        {
            _logger.LogWarning(
                exception,
                "Falha controlada na requisicao {Method} {Path} com status {StatusCode}.",
                context.Request.Method,
                context.Request.Path,
                exception.StatusCode);

            context.Response.StatusCode = exception.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = exception.Message,
                detalhes = exception.Details,
                correlationId
            }));
        }
        catch (BadHttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao desserializar o corpo da requisicao {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = "Falha ao desserializar o corpo da requisicao.",
                detalhes = exception.Message,
                correlationId
            }));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Erro interno nao tratado na requisicao {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = "Erro interno nao tratado.",
                correlationId
            }));
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        return context.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId) &&
               !string.IsNullOrWhiteSpace(correlationId)
            ? correlationId.ToString()
            : context.TraceIdentifier;
    }
}
