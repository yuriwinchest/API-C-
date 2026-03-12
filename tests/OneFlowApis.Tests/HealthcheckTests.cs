using System.Net;
using System.Net.Http.Json;
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
        using var client = _factory.CreateClient();

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
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/oneflow/fiscal/documentos/totais?competencia=2025-01");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task PlanoContas_DeveExigirIdOuPagina()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/oneflow/contabilidade/plano-contas");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }

    [Fact]
    public async Task AliasFiscalDetalhamento_DeveValidarCompetencia()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/oneflow/fiscal/documentos/total/competencia?competencia=2025-01");
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Falha de validacao da requisicao.", payload["mensagem"]?.ToString());
    }
}
