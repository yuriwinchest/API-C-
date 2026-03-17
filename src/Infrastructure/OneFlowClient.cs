using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OneFlowApis.Models;

namespace OneFlowApis.Infrastructure;

public sealed class OneFlowClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OneFlowOptions _options;
    private readonly OneFlowAuthManager _authManager;
    private readonly OneFlowResiliencePolicy _resiliencePolicy;
    private readonly ILogger<OneFlowClient> _logger;

    public OneFlowClient(
        IHttpClientFactory httpClientFactory,
        OneFlowOptions options,
        OneFlowAuthManager authManager,
        OneFlowResiliencePolicy resiliencePolicy,
        ILogger<OneFlowClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _authManager = authManager;
        _resiliencePolicy = resiliencePolicy;
        _logger = logger;
    }

    public async Task<UpstreamResponse> SendAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string?>? query,
        object? body,
        CancellationToken cancellationToken)
    {
        return await _resiliencePolicy.ExecuteAsync(
            method,
            path,
            async innerCancellationToken =>
            {
                var token = _authManager.GetCompanyTokenOrThrow();

                try
                {
                    return await ExecuteAsync(method, path, query, body, token, innerCancellationToken);
                }
                catch (AppException exception) when (exception.StatusCode == 401 && _authManager.CanRefresh)
                {
                    _logger.LogWarning(
                        "Recebido 401 do OneFlow para {Method} {Path}. Tentando renovar o token automaticamente.",
                        method.Method,
                        path);

                    try
                    {
                        var refreshedToken = await _authManager.RefreshCompanyTokenAsync(innerCancellationToken, "401-oneflow");
                        return await ExecuteAsync(method, path, query, body, refreshedToken, innerCancellationToken);
                    }
                    catch (AppException refreshException)
                    {
                        throw new AppException(
                            refreshException.StatusCode >= 500 ? refreshException.StatusCode : 502,
                            "Falha ao renovar automaticamente o token da empresa para consumir o OneFlow.",
                            new
                            {
                                codigo = "oneflow_refresh_automatico_falhou",
                                causa = refreshException.Message,
                                detalhes = refreshException.Details
                            });
                    }
                }
                catch (AppException exception) when (exception.StatusCode == 401)
                {
                    throw new AppException(
                        401,
                        "OneFlow rejeitou a autenticacao da empresa e a renovacao automatica nao esta disponivel.",
                        new
                        {
                            codigo = "oneflow_unauthorized_sem_refresh",
                            acao = "Configure ONEFLOW_COMPANY_TOKEN, ONEFLOW_COMPANY_REFRESH_TOKEN e ONEFLOW_COMPANY_APP_HASH.",
                            detalhes = exception.Details
                        });
                }
                catch (HttpRequestException exception)
                {
                    throw new AppException(502, "Erro de comunicacao com a API do OneFlow.", new
                    {
                        causa = exception.Message
                    });
                }
            },
            cancellationToken);
    }

    private async Task<UpstreamResponse> ExecuteAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string?>? query,
        object? body,
        string token,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var client = _httpClientFactory.CreateClient("OneFlowUpstream");
        var request = new HttpRequestMessage(method, BuildUrl(path, query));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            var payload = body is JsonElement jsonElement
                ? jsonElement.GetRawText()
                : JsonSerializer.Serialize(body, JsonOptions);

            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        var elapsedMs = (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;

        _logger.LogInformation(
            "Chamada OneFlow {Method} {Path} respondeu {StatusCode} em {ElapsedMs} ms.",
            method.Method,
            path,
            (int)response.StatusCode,
            elapsedMs);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AppException(
                    401,
                    "OneFlow rejeitou a autenticacao da empresa.",
                    new
                    {
                        codigo = "oneflow_unauthorized",
                        upstream = TryParseJson(content)
                    });
            }

            throw new AppException(
                (int)response.StatusCode,
                "Falha ao consumir a API do OneFlow.",
                new
                {
                    codigo = "oneflow_upstream_error",
                    upstream = TryParseJson(content)
                });
        }

        return new UpstreamResponse((int)response.StatusCode, content, contentType);
    }

    private string BuildUrl(string path, Dictionary<string, string?>? query)
    {
        var baseUri = _options.OneFlowBaseUrl.TrimEnd('/');
        var url = $"{baseUri}/{path.TrimStart('/')}";

        if (query is null || query.Count == 0)
        {
            return url;
        }

        var queryString = string.Join("&", query
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));

        return string.IsNullOrWhiteSpace(queryString) ? url : $"{url}?{queryString}";
    }

    private static object TryParseJson(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        }
        catch
        {
            return content;
        }
    }
}
