using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OneFlowApis.Infrastructure;
using OneFlowApis.Models;
using OneFlowApis.Services;
using Xunit;

namespace OneFlowApis.Tests;

public sealed class RouteContractTests
{
    [Fact]
    public async Task AliasFiscalDocumentosTotalCompetencia_DeveConsumirEndpointTotais()
    {
        using var context = TestApiContext.Create();
        using var client = context.Factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/fiscal/documentos/total/competencia?competencia=202501");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Get.Method, request.Method);
        Assert.Equal("/api/oneflow/empresa/fiscal/documentos/totais", request.Path);
        Assert.Equal("?competencia=202501", request.Query);
    }

    [Fact]
    public async Task AliasHoleritesTotaisCompetencia_DeveConsumirEndpointRecibosTotais()
    {
        using var context = TestApiContext.Create();
        using var client = context.Factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/folha/holerites/totais/competencia?competencia=202501&tipoFolha=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Get.Method, request.Method);
        Assert.Equal("/api/oneflow/empresa/folha/recibos/totais", request.Path);
        Assert.Equal("?competencia=202501&tipoFolha=1", request.Query);
    }

    [Fact]
    public async Task ObrigacoesGeral_DeveConsumirEndpointGeral()
    {
        using var context = TestApiContext.Create();
        using var client = context.Factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/guias/obrigacoes/geral");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Get.Method, request.Method);
        Assert.Equal("/api/oneflow/empresa/obrigacoes/geral", request.Path);
        Assert.Equal(string.Empty, request.Query);
    }

    [Fact]
    public async Task ObrigacaoAnexo_DeveConsumirEndpointAnexos()
    {
        using var context = TestApiContext.Create();
        using var client = context.Factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/obrigacao/anexo?competencia=202501&codigo=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(context.Handler.Requests);
        Assert.Equal(HttpMethod.Get.Method, request.Method);
        Assert.Equal("/api/oneflow/empresa/obrigacoes/anexos", request.Path);
        Assert.Equal("?competencia=202501&codigo=1", request.Query);
    }

    private static void AddInternalApiKeyHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestApiContext.InternalApiKeyHeaderName);
        client.DefaultRequestHeaders.Add(TestApiContext.InternalApiKeyHeaderName, TestApiContext.InternalApiKey);
    }

    private sealed class TestApiContext : IDisposable
    {
        public const string InternalApiKey = "segredo-interno";
        public const string InternalApiKeyHeaderName = "X-Internal-Api-Key";

        private readonly WebApplicationFactory<Program> _factory;

        private TestApiContext(WebApplicationFactory<Program> factory, CapturingHttpMessageHandler handler)
        {
            _factory = factory;
            Handler = handler;
        }

        public WebApplicationFactory<Program> Factory => _factory;

        public CapturingHttpMessageHandler Handler { get; }

        public static TestApiContext Create()
        {
            var handler = new CapturingHttpMessageHandler();

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<IHttpClientFactory>();
                        services.RemoveAll<OneFlowService>();
                        services.RemoveAll<OneFlowClient>();
                        services.RemoveAll<OneFlowAuthManager>();
                        services.RemoveAll<OneFlowResiliencePolicy>();
                        services.RemoveAll<OneFlowOptions>();
                        services.RemoveAll<InternalApiSecurityOptions>();
                        services.RemoveAll<OneFlowResilienceOptions>();

                        services.AddSingleton<IHttpClientFactory>(new FakeHttpClientFactory(handler));
                        services.AddSingleton(new OneFlowOptions
                        {
                            Port = 3000,
                            OneFlowBaseUrl = "https://oneflow.test/api",
                            OmiePortalAppsBaseUrl = "https://omie.test/api/portal/apps",
                            CompanyToken = "token-teste",
                            CompanyRefreshToken = "refresh-teste",
                            CompanyAppHash = "app-hash-teste"
                        });
                        services.AddSingleton(new InternalApiSecurityOptions
                        {
                            HeaderName = InternalApiKeyHeaderName,
                            ApiKey = InternalApiKey
                        });
                        services.AddSingleton(new OneFlowResilienceOptions());
                        services.AddSingleton<OneFlowResiliencePolicy>();
                        services.AddSingleton<OneFlowAuthManager>();
                        services.AddSingleton<OneFlowClient>();
                        services.AddSingleton<OneFlowService>();
                    });
                });

            return new TestApiContext(factory, handler);
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, disposeHandler: false);
        }
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<CapturedRequest> _requests = [];

        public IReadOnlyList<CapturedRequest> Requests => _requests;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            _requests.Add(new CapturedRequest(
                request.Method.Method,
                request.RequestUri?.AbsolutePath ?? string.Empty,
                request.RequestUri?.Query ?? string.Empty,
                body));

            var payload = "{\"status\":\"ok\"}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record CapturedRequest(string Method, string Path, string Query, string? Body);
}
