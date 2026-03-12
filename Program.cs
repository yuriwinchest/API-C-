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

api.MapPost("/folha/variaveis", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
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

api.MapPost("/contabilidade/lancamentos", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
{
    var response = await service.PostLancamentoContabilAsync(body, cancellationToken);
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

api.MapGet("/guias/anexos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
{
    var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
    var codigo = RequestValidators.RequiredString(request.Query["codigo"], "codigo");

    var response = await service.GetObrigacoesAnexosAsync(competencia, codigo, cancellationToken);
    return response.ToResult();
});

app.Run();

public partial class Program;
