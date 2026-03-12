using System.Text.Json;
using OneFlowApis.Services;
using OneFlowApis.Validation;

namespace OneFlowApis.Endpoints;

public static class FolhaEndpoints
{
    public static RouteGroupBuilder MapFolhaEndpoints(this RouteGroupBuilder api)
    {
        var folha = api.MapGroup("/folha").WithTags("Folha");

        folha.MapPost("/variaveis", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostVariaveisFolhaAsync(body, cancellationToken);
            return response.ToResult();
        });

        folha.MapPost("/variaveisfolha", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostVariaveisFolhaAsync(body, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/trabalhadores/dados-basicos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
            var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
            RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

            var response = await service.GetTrabalhadorDadosBasicosAsync(competencia, cpf, matricula, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/dadosbasicostrabalhador/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
            var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
            RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

            var response = await service.GetTrabalhadorDadosBasicosAsync(competencia, cpf, matricula, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/trabalhadores/eventos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
            var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
            var idEvento = RequestValidators.OptionalString(request.Query["idEvento"]);
            RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

            var response = await service.GetTrabalhadorEventosAsync(competencia, cpf, matricula, idEvento, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/rubricas/dados-basicos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var rubrica = RequestValidators.OptionalString(request.Query["rubrica"]);
            var response = await service.GetRubricasDadosBasicosAsync(competencia, rubrica, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/eventosdostrabalhadores/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var cpf = RequestValidators.OptionalString(request.Query["cpf"]);
            var matricula = RequestValidators.OptionalString(request.Query["matricula"]);
            var idEvento = RequestValidators.OptionalString(request.Query["idEvento"]);
            RequestValidators.RequireAtLeastOne(cpf, matricula, "cpf", "matricula");

            var response = await service.GetTrabalhadorEventosAsync(competencia, cpf, matricula, idEvento, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/holerites/totais", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var tipoFolha = RequestValidators.RequiredPositiveInt(request.Query["tipoFolha"], "tipoFolha");

            var response = await service.GetHoleritesTotaisAsync(competencia, tipoFolha, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/datas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDatasFolhaAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/datasconfigurada/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDatasFolhaAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/status", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetStatusFolhaAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/statusfolha/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetStatusFolhaAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/fator-r", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetFatorRAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/fatorr/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetFatorRAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        folha.MapGet("/recibos/totais/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var tipoFolha = RequestValidators.RequiredPositiveInt(request.Query["tipoFolha"], "tipoFolha");

            var response = await service.GetHoleritesTotaisAsync(competencia, tipoFolha, cancellationToken);
            return response.ToResult();
        });

        return api;
    }
}
