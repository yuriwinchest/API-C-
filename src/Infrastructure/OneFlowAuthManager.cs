using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OneFlowApis.Models;

namespace OneFlowApis.Infrastructure;

public sealed class OneFlowAuthManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OneFlowOptions _options;
    private readonly ILogger<OneFlowAuthManager> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _token;
    private string? _refreshToken;

    public OneFlowAuthManager(
        IHttpClientFactory httpClientFactory,
        OneFlowOptions options,
        ILogger<OneFlowAuthManager> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
        _token = options.CompanyToken;
        _refreshToken = options.CompanyRefreshToken;
    }

    public bool CanRefresh =>
        !string.IsNullOrWhiteSpace(_token) &&
        !string.IsNullOrWhiteSpace(_refreshToken) &&
        !string.IsNullOrWhiteSpace(_options.CompanyAppHash);

    public string GetCompanyTokenOrThrow()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            throw new AppException(
                500,
                "Variavel ONEFLOW_COMPANY_TOKEN nao configurada. Preencha o token JWT da empresa para habilitar a integracao.");
        }

        return _token;
    }

    public async Task<string> RefreshCompanyTokenAsync(CancellationToken cancellationToken)
    {
        if (!CanRefresh)
        {
            throw new AppException(
                500,
                "Renovacao automatica indisponivel. Configure ONEFLOW_COMPANY_TOKEN, ONEFLOW_COMPANY_REFRESH_TOKEN e ONEFLOW_COMPANY_APP_HASH.");
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Iniciando renovacao do token da empresa no portal Omie.");

            var client = _httpClientFactory.CreateClient("OmiePortal");
            var baseUrl = _options.OmiePortalAppsBaseUrl.TrimEnd('/');
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl}/{_options.CompanyAppHash}/refresh-token");

            var payload = JsonSerializer.Serialize(new
            {
                token = _token,
                refresh_token = _refreshToken
            });

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new AppException(
                    (int)response.StatusCode,
                    "Falha ao renovar o token da empresa no portal Omie.",
                    TryParseJson(content));
            }

            var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, JsonOptions);
            if (refreshResponse is null ||
                string.IsNullOrWhiteSpace(refreshResponse.Token) ||
                string.IsNullOrWhiteSpace(refreshResponse.RefreshToken))
            {
                throw new AppException(502, "Resposta invalida ao renovar o token da empresa.", TryParseJson(content));
            }

            _token = refreshResponse.Token;
            _refreshToken = refreshResponse.RefreshToken;

            _logger.LogInformation("Token da empresa renovado com sucesso.");

            return _token;
        }
        finally
        {
            _refreshLock.Release();
        }
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

    private sealed class RefreshTokenResponse
    {
        public string? Token { get; init; }

        public string? RefreshToken { get; init; }
    }
}
