using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

    public OneFlowClient(
        IHttpClientFactory httpClientFactory,
        OneFlowOptions options,
        OneFlowAuthManager authManager)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _authManager = authManager;
    }

    public async Task<UpstreamResponse> SendAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string?>? query,
        object? body,
        CancellationToken cancellationToken)
    {
        var token = _authManager.GetCompanyTokenOrThrow();

        try
        {
            return await ExecuteAsync(method, path, query, body, token, cancellationToken);
        }
        catch (AppException exception) when (exception.StatusCode == 401 && _authManager.CanRefresh)
        {
            var refreshedToken = await _authManager.RefreshCompanyTokenAsync(cancellationToken);
            return await ExecuteAsync(method, path, query, body, refreshedToken, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new AppException(502, "Erro de comunicacao com a API do OneFlow.", new
            {
                causa = exception.Message
            });
        }
    }

    private async Task<UpstreamResponse> ExecuteAsync(
        HttpMethod method,
        string path,
        Dictionary<string, string?>? query,
        object? body,
        string token,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
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

        if (!response.IsSuccessStatusCode)
        {
            throw new AppException((int)response.StatusCode, "Falha ao consumir a API do OneFlow.", TryParseJson(content));
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
