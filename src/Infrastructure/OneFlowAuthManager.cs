using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly IOneFlowCredentialStore _credentialStore;
    private readonly ILogger<OneFlowAuthManager> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly object _diagnosticsLock = new();

    private string? _token;
    private string? _refreshToken;
    private DateTimeOffset? _lastRefreshAttemptAtUtc;
    private DateTimeOffset? _lastRefreshSucceededAtUtc;
    private DateTimeOffset? _lastRefreshFailedAtUtc;
    private string? _lastRefreshTrigger;
    private string? _lastRefreshResult;
    private string? _lastRefreshFailureMessage;

    public OneFlowAuthManager(
        IHttpClientFactory httpClientFactory,
        OneFlowOptions options,
        IOneFlowCredentialStore credentialStore,
        ILogger<OneFlowAuthManager> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _credentialStore = credentialStore;
        _logger = logger;
        _token = options.CompanyToken;
        _refreshToken = options.CompanyRefreshToken;
    }

    public bool CanRefresh =>
        !string.IsNullOrWhiteSpace(_token) &&
        !string.IsNullOrWhiteSpace(_refreshToken) &&
        !string.IsNullOrWhiteSpace(_options.CompanyAppHash);

    public OneFlowAuthDiagnostics GetDiagnostics()
    {
        string? token;
        string? refreshToken;
        DateTimeOffset? lastRefreshAttemptAtUtc;
        DateTimeOffset? lastRefreshSucceededAtUtc;
        DateTimeOffset? lastRefreshFailedAtUtc;
        string? lastRefreshTrigger;
        string? lastRefreshResult;
        string? lastRefreshFailureMessage;

        lock (_diagnosticsLock)
        {
            token = _token;
            refreshToken = _refreshToken;
            lastRefreshAttemptAtUtc = _lastRefreshAttemptAtUtc;
            lastRefreshSucceededAtUtc = _lastRefreshSucceededAtUtc;
            lastRefreshFailedAtUtc = _lastRefreshFailedAtUtc;
            lastRefreshTrigger = _lastRefreshTrigger;
            lastRefreshResult = _lastRefreshResult;
            lastRefreshFailureMessage = _lastRefreshFailureMessage;
        }

        var tokenExpirationUtc = TryGetTokenExpirationUtc(token);
        var now = DateTimeOffset.UtcNow;
        var missingSettings = GetMissingRefreshSettings();
        var tokenDiagnostics = new OneFlowTokenDiagnostics(
            Configurado: !string.IsNullOrWhiteSpace(token),
            Identificador: BuildTokenFingerprint(token),
            FormatoJwtReconhecido: tokenExpirationUtc.HasValue,
            Expirado: tokenExpirationUtc.HasValue && tokenExpirationUtc.Value <= now,
            ExpiraEmUtc: tokenExpirationUtc,
            SegundosRestantes: tokenExpirationUtc.HasValue
                ? Math.Max(0, (long)Math.Floor((tokenExpirationUtc.Value - now).TotalSeconds))
                : null);

        var refreshDiagnostics = new OneFlowRefreshDiagnostics(
            Habilitada: !string.IsNullOrWhiteSpace(token) &&
                       !string.IsNullOrWhiteSpace(refreshToken) &&
                       !string.IsNullOrWhiteSpace(_options.CompanyAppHash),
            ConfiguracoesAusentes: missingSettings,
            UltimoGatilho: lastRefreshTrigger,
            UltimoResultado: lastRefreshResult,
            UltimaTentativaEmUtc: lastRefreshAttemptAtUtc,
            UltimoSucessoEmUtc: lastRefreshSucceededAtUtc,
            UltimaFalhaEmUtc: lastRefreshFailedAtUtc,
            UltimaFalhaMensagem: lastRefreshFailureMessage);

        return new OneFlowAuthDiagnostics(
            tokenDiagnostics,
            refreshDiagnostics,
            _credentialStore.GetDiagnostics());
    }

    public string GetCompanyTokenOrThrow()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            throw new AppException(
                500,
                "Variavel ONEFLOW_COMPANY_TOKEN nao configurada. Preencha o token JWT da empresa para habilitar a integracao.",
                new
                {
                    codigo = "oneflow_token_nao_configurado",
                    acao = "Preencha a variavel ONEFLOW_COMPANY_TOKEN no .env ou no ambiente do servidor."
                });
        }

        return _token;
    }

    public async Task<string> RefreshCompanyTokenAsync(CancellationToken cancellationToken, string trigger = "manual")
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            lock (_diagnosticsLock)
            {
                _lastRefreshAttemptAtUtc = DateTimeOffset.UtcNow;
                _lastRefreshTrigger = trigger;
                _lastRefreshResult = "em_andamento";
                _lastRefreshFailureMessage = null;
            }

            var missingSettings = GetMissingRefreshSettings();
            if (missingSettings.Count > 0)
            {
                throw new AppException(
                    500,
                    "Renovacao automatica indisponivel. Faltam credenciais obrigatorias do OneFlow.",
                    new
                    {
                        codigo = "oneflow_refresh_nao_configurado",
                        configuracoesAusentes = missingSettings,
                        acao = "Preencha ONEFLOW_COMPANY_TOKEN, ONEFLOW_COMPANY_REFRESH_TOKEN e ONEFLOW_COMPANY_APP_HASH."
                    });
            }

            _logger.LogInformation(
                "Iniciando renovacao do token da empresa no portal Omie. Gatilho: {Trigger}.",
                trigger);

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
                    new
                    {
                        codigo = "oneflow_refresh_rejeitado_pelo_portal",
                        upstream = TryParseJson(content)
                    });
            }

            var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content, JsonOptions);
            if (refreshResponse is null ||
                string.IsNullOrWhiteSpace(refreshResponse.Token) ||
                string.IsNullOrWhiteSpace(refreshResponse.RefreshToken))
            {
                throw new AppException(
                    502,
                    "Resposta invalida ao renovar o token da empresa.",
                    new
                    {
                        codigo = "oneflow_refresh_resposta_invalida",
                        upstream = TryParseJson(content)
                    });
            }

            _token = refreshResponse.Token;
            _refreshToken = refreshResponse.RefreshToken;
            _credentialStore.PersistRefreshedCredentials(_token, _refreshToken);
            lock (_diagnosticsLock)
            {
                _lastRefreshSucceededAtUtc = DateTimeOffset.UtcNow;
                _lastRefreshResult = "sucesso";
                _lastRefreshFailureMessage = null;
            }

            _logger.LogInformation(
                "Token da empresa renovado com sucesso. Gatilho: {Trigger}.",
                trigger);

            return _token;
        }
        catch (AppException exception)
        {
            lock (_diagnosticsLock)
            {
                _lastRefreshFailedAtUtc = DateTimeOffset.UtcNow;
                _lastRefreshResult = "falha";
                _lastRefreshFailureMessage = exception.Message;
            }

            _logger.LogWarning(
                exception,
                "Falha ao renovar o token da empresa. Gatilho: {Trigger}.",
                trigger);

            throw;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private IReadOnlyList<string> GetMissingRefreshSettings()
    {
        var missingSettings = new List<string>();

        if (string.IsNullOrWhiteSpace(_token))
        {
            missingSettings.Add("ONEFLOW_COMPANY_TOKEN");
        }

        if (string.IsNullOrWhiteSpace(_refreshToken))
        {
            missingSettings.Add("ONEFLOW_COMPANY_REFRESH_TOKEN");
        }

        if (string.IsNullOrWhiteSpace(_options.CompanyAppHash))
        {
            missingSettings.Add("ONEFLOW_COMPANY_APP_HASH");
        }

        return missingSettings;
    }

    private static DateTimeOffset? TryGetTokenExpirationUtc(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var segments = token.Split('.');
        if (segments.Length < 2)
        {
            return null;
        }

        try
        {
            var payloadBytes = DecodeBase64Url(segments[1]);
            var payload = JsonSerializer.Deserialize<JsonElement>(payloadBytes, JsonOptions);

            if (!payload.TryGetProperty("exp", out var expirationElement))
            {
                return null;
            }

            return expirationElement.ValueKind switch
            {
                JsonValueKind.Number when expirationElement.TryGetInt64(out var seconds) => DateTimeOffset.FromUnixTimeSeconds(seconds),
                JsonValueKind.String when long.TryParse(expirationElement.GetString(), out var seconds) => DateTimeOffset.FromUnixTimeSeconds(seconds),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? BuildTokenFingerprint(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash)[..12];
    }

    private static byte[] DecodeBase64Url(string input)
    {
        var normalized = input.Replace('-', '+').Replace('_', '/');
        var padding = normalized.Length % 4;

        if (padding > 0)
        {
            normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
        }

        return Convert.FromBase64String(normalized);
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
        [JsonPropertyName("token")]
        public string? Token { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }
    }
}
