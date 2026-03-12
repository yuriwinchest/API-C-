using System.Text.Json;
using OneFlowApis.Models;

namespace OneFlowApis.Middlewares;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException exception)
        {
            context.Response.StatusCode = exception.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = exception.Message,
                detalhes = exception.Details
            }));
        }
        catch (BadHttpRequestException exception)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = "Falha ao desserializar o corpo da requisicao.",
                detalhes = exception.Message
            }));
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                mensagem = "Erro interno nao tratado."
            }));
        }
    }
}
