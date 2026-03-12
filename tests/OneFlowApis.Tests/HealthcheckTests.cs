using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OneFlowApis.Tests;

public sealed class HealthcheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthcheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Healthcheck_DeveResponderOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("ok", payload["status"]);
        Assert.Equal("one-flow-apis", payload["servico"]);
    }

    [Fact]
    public async Task DocumentosTotais_DeveValidarCompetencia()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/fiscal/documentos/totais?competencia=2025-01");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task PlanoContas_DeveExigirIdOuPagina()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/contabilidade/plano-contas");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task AliasFiscalDetalhamento_DeveValidarCompetencia()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/fiscal/documentos/total/competencia?competencia=2025-01");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task ApuracoesResumo_DeveExigirImposto()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/fiscal/apuracoes/resumo?competencia=202501");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task ConfiguracaoStatus_DeveResponderSemExporSegredos()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/configuracao/status");
        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(payload.RootElement.TryGetProperty("oneFlow", out var oneFlow));
        Assert.True(oneFlow.TryGetProperty("tokenConfigurado", out _));
        Assert.False(oneFlow.TryGetProperty("token", out _));
        Assert.True(payload.RootElement.TryGetProperty("gClick", out _));
        Assert.True(payload.RootElement.TryGetProperty("omie", out _));
        Assert.True(payload.RootElement.TryGetProperty("autenticacaoInterna", out _));
        Assert.True(payload.RootElement.TryGetProperty("resilienciaHttp", out _));
    }

    [Fact]
    public async Task SwaggerJson_DeveResponderOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Scalar_DeveResponderOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointInterno_DeveExigirChaveQuandoConfigurada()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/oneflow/configuracao/status");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointInterno_DeveAceitarChaveQuandoConfigurada()
    {
        using var environment = new TemporaryEnvironmentVariable("INTERNAL_API_KEY", "segredo-interno");
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        AddInternalApiKeyHeader(client);

        var response = await client.GetAsync("/api/oneflow/configuracao/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static void AddInternalApiKeyHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", "segredo-interno");
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>();
    }

    private sealed class TemporaryEnvironmentVariable : IDisposable
    {
        private readonly string _key;
        private readonly string? _previousValue;

        public TemporaryEnvironmentVariable(string key, string? value)
        {
            _key = key;
            _previousValue = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_key, _previousValue);
        }
    }
}
