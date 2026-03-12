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

    public Task<UpstreamResponse> GetDocumentosQuantidadeAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/fiscal/documentos/quantidade",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetDocumentosListarAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/fiscal/documentos/listar",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetApuracoesFiscaisAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/fiscal/apuracao/listar",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetAliquotasSimplesNacionalAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/fiscal/simplesnacional/aliquotas",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> PostFiscalNfseNacionalAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/fiscal/nfse/nacional",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostFiscalNfsePrefeituraAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/fiscal/nfse/prefeitura",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostFiscalDocumentoRemoverAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/fiscal/documentos/remover",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostFiscalDocumentoAlterarStatusAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/fiscal/documentos/alterarstatus",
            null,
            body,
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

    public Task<UpstreamResponse> GetStatusFolhaAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/statusfolha",
            new Dictionary<string, string?> { ["competencia"] = competencia },
            null,
            cancellationToken);

    public Task<UpstreamResponse> GetFatorRAsync(string competencia, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/folha/fatorr",
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

    public Task<UpstreamResponse> PostLancamentoContabilTransacaoAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/gerartransacao",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostLancamentoContabilPadraoAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/gerarpadrao",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostExcluirLancamentoContabilAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/excluirlancamento",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostExcluirLancamentoContabilTransacaoAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/excluirtransacao",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> PostExcluirLancamentoContabilPadraoAsync(JsonElement body, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Post,
            "oneflow/empresa/contabil/lancamentos/excluirpadrao",
            null,
            body,
            cancellationToken);

    public Task<UpstreamResponse> GetPlanoContasAsync(int? id, int? pagina, CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/contabil/planocontas/contas",
            new Dictionary<string, string?>
            {
                ["id"] = id?.ToString(),
                ["pagina"] = pagina?.ToString()
            },
            null,
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

    public Task<UpstreamResponse> GetRazaoAsync(
        string conta,
        string competenciaInicial,
        string competenciaFinal,
        CancellationToken cancellationToken) =>
        _client.SendAsync(
            HttpMethod.Get,
            "oneflow/empresa/contabil/razao",
            new Dictionary<string, string?>
            {
                ["conta"] = conta,
                ["competenciaInicial"] = competenciaInicial,
                ["competenciaFinal"] = competenciaFinal
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
