using System.Text.Json;
using OneFlowApis.Infrastructure;
using OneFlowApis.Models;

namespace OneFlowApis.Services;

public sealed class OneFlowService
{
    private readonly OneFlowClient _client;

    public OneFlowService(OneFlowClient client)
    {
        _client = client;
    }

    public Task<UpstreamResponse> GetDocumentosTotaisAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/fiscal/documentos/totais",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> PostVariaveisFolhaAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/folha/variaveis/incluir",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> GetTrabalhadorDadosBasicosAsync(
        string competencia,
        string? cpf,
        string? matricula,
        CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/trabalhador/dadosbasicos",
            new Dictionary<string, string?>
            {
                ["competencia"] = competencia,
                ["cpf"] = cpf,
                ["matricula"] = matricula
            },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetTrabalhadorEventosAsync(
        string competencia,
        string? cpf,
        string? matricula,
        string? idEvento,
        CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/trabalhador/eventos",
            new Dictionary<string, string?>
            {
                ["competencia"] = competencia,
                ["cpf"] = cpf,
                ["matricula"] = matricula,
                ["idEvento"] = idEvento
            },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetHoleritesTotaisAsync(string competencia, int tipoFolha, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/recibos/totais",
            new Dictionary<string, string?>
            {
                ["competencia"] = competencia,
                ["tipoFolha"] = tipoFolha.ToString()
            },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetDatasFolhaAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/datas",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> PostLancamentoContabilAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/gerarlancamento",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> GetBalanceteAsync(
        string competenciaInicial,
        string competenciaFinal,
        string zeramento,
        CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/contabil/balancete",
            new Dictionary<string, string?>
            {
                ["competenciaInicial"] = competenciaInicial,
                ["competenciaFinal"] = competenciaFinal,
                ["zeramento"] = zeramento
            },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetObrigacoesAnexosAsync(string competencia, string codigo, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/obrigacoes/anexos",
            new Dictionary<string, string?>
            {
                ["competencia"] = competencia,
                ["codigo"] = codigo
            },
            null,
            cancellationToken);
}
