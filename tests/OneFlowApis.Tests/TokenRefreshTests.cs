using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OneFlowApis.Infrastructure;
using OneFlowApis.Models;
using Xunit;

namespace OneFlowApis.Tests;

public sealed class TokenRefreshTests
{
    [Fact]
    public void DotEnvOneFlowCredentialStore_DeveAtualizarTokenERefreshTokenNoArquivo()
    {
        var tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            var envPath = Path.Combine(tempDirectory.FullName, ".env");
            File.WriteAllLines(envPath,
            [
                "PORT=3000",
                "ONEFLOW_COMPANY_TOKEN=token-antigo",
                "ONEFLOW_COMPANY_REFRESH_TOKEN=refresh-antigo",
                "ONEFLOW_COMPANY_APP_HASH=app-hash"
            ]);

            var logger = LoggerFactory.Create(_ => { }).CreateLogger<DotEnvOneFlowCredentialStore>();
            var store = new DotEnvOneFlowCredentialStore(logger, envPath);

            store.PersistRefreshedCredentials("token-novo", "refresh-novo");

            var lines = File.ReadAllLines(envPath);

            Assert.Contains("ONEFLOW_COMPANY_TOKEN=token-novo", lines);
            Assert.Contains("ONEFLOW_COMPANY_REFRESH_TOKEN=refresh-novo", lines);
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task OneFlowClient_DeveRenovarTokenAutomaticamenteQuandoReceber401()
    {
        var upstreamHandler = new SequencedHttpMessageHandler();
        upstreamHandler.Enqueue(request =>
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("token-expirado", request.Headers.Authorization?.Parameter);

            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"message\":\"jwt expired\"}", Encoding.UTF8, "application/json")
            };
        });
        upstreamHandler.Enqueue(request =>
        {
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
            Assert.Equal("token-renovado", request.Headers.Authorization?.Parameter);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
            };
        });

        var omieHandler = new SequencedHttpMessageHandler();
        omieHandler.Enqueue(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/portal/apps/app-hash/refresh-token", request.RequestUri?.AbsolutePath);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"token\":\"token-renovado\",\"refresh_token\":\"refresh-renovado\"}",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var httpClientFactory = new NamedHttpClientFactory(new Dictionary<string, HttpClient>
        {
            ["OneFlowUpstream"] = CreateClient("https://oneflow.test/api/", upstreamHandler),
            ["OmiePortal"] = CreateClient("https://omie.test/api/portal/apps/", omieHandler)
        });

        var credentialStore = new RecordingCredentialStore();
        var loggerFactory = LoggerFactory.Create(_ => { });
        var options = new OneFlowOptions
        {
            Port = 3000,
            OneFlowBaseUrl = "https://oneflow.test/api",
            OmiePortalAppsBaseUrl = "https://omie.test/api/portal/apps",
            CompanyToken = "token-expirado",
            CompanyRefreshToken = "refresh-expirado",
            CompanyAppHash = "app-hash"
        };
        var authManager = new OneFlowAuthManager(
            httpClientFactory,
            options,
            credentialStore,
            loggerFactory.CreateLogger<OneFlowAuthManager>());
        var resiliencePolicy = new OneFlowResiliencePolicy(
            new OneFlowResilienceOptions(),
            loggerFactory.CreateLogger<OneFlowResiliencePolicy>());
        var client = new OneFlowClient(
            httpClientFactory,
            options,
            authManager,
            resiliencePolicy,
            loggerFactory.CreateLogger<OneFlowClient>());

        var response = await client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/geral/dadosbasicos",
            null,
            null,
            CancellationToken.None);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, upstreamHandler.Requests.Count);
        Assert.Single(omieHandler.Requests);
        Assert.Equal("token-renovado", credentialStore.Token);
        Assert.Equal("refresh-renovado", credentialStore.RefreshToken);
    }

    [Fact]
    public void OneFlowAuthManager_DeveExporDiagnosticoDoToken()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        var httpClientFactory = new NamedHttpClientFactory(new Dictionary<string, HttpClient>
        {
            ["OneFlowUpstream"] = CreateClient("https://oneflow.test/api/", new SequencedHttpMessageHandler()),
            ["OmiePortal"] = CreateClient("https://omie.test/api/portal/apps/", new SequencedHttpMessageHandler())
        });
        var options = new OneFlowOptions
        {
            Port = 3000,
            OneFlowBaseUrl = "https://oneflow.test/api",
            OmiePortalAppsBaseUrl = "https://omie.test/api/portal/apps",
            CompanyToken = CreateJwt(DateTimeOffset.UtcNow.AddMinutes(30)),
            CompanyRefreshToken = "refresh-valido",
            CompanyAppHash = "app-hash"
        };

        var authManager = new OneFlowAuthManager(
            httpClientFactory,
            options,
            new RecordingCredentialStore(),
            loggerFactory.CreateLogger<OneFlowAuthManager>());

        var diagnostics = authManager.GetDiagnostics();

        Assert.True(diagnostics.Token.Configurado);
        Assert.True(diagnostics.Token.FormatoJwtReconhecido);
        Assert.NotNull(diagnostics.Token.Identificador);
        Assert.False(diagnostics.Token.Expirado);
        Assert.True(diagnostics.RenovacaoAutomatica.Habilitada);
        Assert.Empty(diagnostics.RenovacaoAutomatica.ConfiguracoesAusentes);
    }

    private static HttpClient CreateClient(string baseAddress, HttpMessageHandler handler)
    {
        return new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri(baseAddress)
        };
    }

    private sealed class RecordingCredentialStore : IOneFlowCredentialStore
    {
        public string? Token { get; private set; }

        public string? RefreshToken { get; private set; }

        public void PersistRefreshedCredentials(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }

        public OneFlowCredentialStoreDiagnostics GetDiagnostics()
        {
            return new OneFlowCredentialStoreDiagnostics(
                "somente-memoria",
                false,
                false,
                Token is not null && RefreshToken is not null,
                null,
                null);
        }
    }

    private sealed class NamedHttpClientFactory : IHttpClientFactory
    {
        private readonly IReadOnlyDictionary<string, HttpClient> _clients;

        public NamedHttpClientFactory(IReadOnlyDictionary<string, HttpClient> clients)
        {
            _clients = clients;
        }

        public HttpClient CreateClient(string name)
        {
            return _clients[name];
        }
    }

    private sealed class SequencedHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = [];

        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responses.Enqueue(responseFactory);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("Nenhuma resposta configurada para o handler.");
            }

            return Task.FromResult(_responses.Dequeue()(request));
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                var body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                clone.Content = new StringContent(body, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.Remove(header.Key);
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }
    }

    private static string CreateJwt(DateTimeOffset expiresAtUtc)
    {
        var header = Base64UrlEncode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncode(JsonSerializer.Serialize(new
        {
            exp = expiresAtUtc.ToUnixTimeSeconds(),
            sub = "empresa-teste"
        }));

        return $"{header}.{payload}.assinatura";
    }

    private static string Base64UrlEncode(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
