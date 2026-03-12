using System.Text.Json;
using OneFlowApis.Infrastructure;
using OneFlowApis.Middlewares;
using OneFlowApis.Models;
using OneFlowApis.Services;
using OneFlowApis.Validation;

DotEnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);
var oneFlowOptions = OneFlowOptions.FromConfiguration(builder.Configuration);

builder.WebHost.UseUrls($"http://0.0.0.0:{oneFlowOptions.Port}");

builder.Services.AddSingleton(oneFlowOptions);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<OneFlowAuthManager>();
builder.Services.AddSingleton<OneFlowClient>();
builder.Services.AddSingleton<OneFlowService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    servico = "one-flow-apis"
}));

var api = app.MapGroup("/api/oneflow");

api.MapGet("/fiscal/documentos/totais", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDocumentosTotaisAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/total/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDocumentosTotaisAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/documentos/quantidade", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDocumentosQuantidadeAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/documentos/listar", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDocumentosListarAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/documentos/total/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDocumentosListarAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/apuracoes", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetApuracoesFiscaisAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/apuracao/competencia/imposto", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetApuracoesFiscaisAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/simples-nacional/aliquotas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetAliquotasSimplesNacionalAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/fiscal/aliquotas/simplesnacional/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetAliquotasSimplesNacionalAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfse/nacional", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalNfseNacionalAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfsenacional/incluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalNfseNacionalAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfse/prefeitura", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalNfsePrefeituraAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfseprefeitura/incluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalNfsePrefeituraAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/documentos/remover", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalDocumentoRemoverAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfse/remove", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalDocumentoRemoverAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/documentos/alterar-status", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalDocumentoAlterarStatusAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/fiscal/nfsestatus/alterar", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostFiscalDocumentoAlterarStatusAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/folha/variaveis", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostVariaveisFolhaAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/folha/variaveisfolha", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostVariaveisFolhaAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/trabalhadores/dados-basicos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
    var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
    RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

    var response = await service.GetTrabalhadorDadosBasicosAsync(competencia, cpf, matricula, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/dadosbasicostrabalhador/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
    var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
    RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

    var response = await service.GetTrabalhadorDadosBasicosAsync(competencia, cpf, matricula, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/trabalhadores/eventos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
    var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
    var idEvento = RequestValidators.OptionalString(request.Query["idEvento"]);
    RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

    var response = await service.GetTrabalhadorEventosAsync(competencia, cpf, matricula, idEvento, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/eventosdostrabalhadores/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
    var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
    var idEvento = RequestValidators.OptionalString(request.Query["idEvento"]);
    RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

    var response = await service.GetTrabalhadorEventosAsync(competencia, cpf, matricula, idEvento, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/holerites/totais", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var tipoFolha = RequestValidators.RequiredPositiveInt(request.Query["tipoFolha"], "tipoFolha");

    var response = await service.GetHoleritesTotaisAsync(competencia, tipoFolha, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/datas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDatasFolhaAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/datasconfigurada/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetDatasFolhaAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/status", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetStatusFolhaAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/statusfolha/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetStatusFolhaAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/fator-r", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetFatorRAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/fatorr/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var response = await service.GetFatorRAsync(competencia, cancellationToken);
    return response.ToResult();
});

api.MapGet("/folha/recibos/totais/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var tipoFolha = RequestValidators.RequiredPositiveInt(request.Query["tipoFolha"], "tipoFolha");

    var response = await service.GetHoleritesTotaisAsync(competencia, tipoFolha, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostLancamentoContabilAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos/transacao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostLancamentoContabilTransacaoAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos/padrao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostLancamentoContabilPadraoAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos/excluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostExcluirLancamentoContabilAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos/excluir-transacao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostExcluirLancamentoContabilTransacaoAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapPost("/contabilidade/lancamentos/excluir-padrao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostExcluirLancamentoContabilPadraoAsync(body, cancellationToken);
    return response.ToResult();
});

api.MapGet("/contabilidade/plano-contas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var id = RequestValidators.OptionalPositiveInt(request.Query["id"], "id");
    var pagina = RequestValidators.OptionalPositiveInt(request.Query["pagina"], "pagina");
    RequestValidators.RequireAtLeastOne(id, pagina, "id", "pagina");

    var response = await service.GetPlanoContasAsync(id, pagina, cancellationToken);
    return response.ToResult();
});

api.MapGet("/contabilidade/planocontas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var id = RequestValidators.OptionalPositiveInt(request.Query["id"], "id");
    var pagina = RequestValidators.OptionalPositiveInt(request.Query["pagina"], "pagina");
    RequestValidators.RequireAtLeastOne(id, pagina, "id", "pagina");

    var response = await service.GetPlanoContasAsync(id, pagina, cancellationToken);
    return response.ToResult();
});

api.MapGet("/contabilidade/balancete", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
    var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");
    var zeramento = RequestValidators.RequiredAllowedValue(request.Query["zeramento"], "zeramento", ["S", "N"]);

    var response = await service.GetBalanceteAsync(competenciaInicial, competenciaFinal, zeramento, cancellationToken);
    return response.ToResult();
});

api.MapGet("/contabilidade/razao", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var conta = RequestValidators.RequiredString(request.Query["conta"], "conta");
    var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
    var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");

    var response = await service.GetRazaoAsync(conta, competenciaInicial, competenciaFinal, cancellationToken);
    return response.ToResult();
});

api.MapGet("/contabilidade/lancamentos/conta", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var conta = RequestValidators.RequiredString(request.Query["conta"], "conta");
    var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
    var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");

    var response = await service.GetRazaoAsync(conta, competenciaInicial, competenciaFinal, cancellationToken);
    return response.ToResult();
});

api.MapGet("/guias/anexos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var codigo = RequestValidators.RequiredString(request.Query["codigo"], "codigo");

    var response = await service.GetObrigacoesAnexosAsync(competencia, codigo, cancellationToken);
    return response.ToResult();
});

app.Run();

public partial class Program;
