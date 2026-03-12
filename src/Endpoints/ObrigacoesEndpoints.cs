using System.Text.Json;
using OneFlowApis.Services;
using OneFlowApis.Validation;

namespace OneFlowApis.Endpoints;

public static class ObrigacoesEndpoints
{
    public static RouteGroupBuilder MapObrigacoesEndpoints(this RouteGroupBuilder api)
    {
        var guias = api.MapGroup("/guias").WithTags("Obrigacoes");

        guias.MapGet("/anexos", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var codigo = RequestValidators.RequiredString(request.Query["codigo"], "codigo");

            var response = await service.GetObrigacoesAnexosAsync(competencia, codigo, cancellationToken);
            return response.ToResult();
        });

        guias.MapGet("/obrigacoes/geral", async (OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetObrigacoesGeralAsync(cancellationToken);
            return response.ToResult();
        });

        guias.MapGet("/obrigacoes/listar", async (OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetObrigacoesListarAsync(cancellationToken);
            return response.ToResult();
        });

        guias.MapPost("/obrigacoes/incluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostObrigacoesIncluirAsync(body, cancellationToken);
            return response.ToResult();
        });

        api.MapGet("/obrigacao/anexo", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var codigo = RequestValidators.RequiredString(request.Query["codigo"], "codigo");

            var response = await service.GetObrigacoesAnexosAsync(competencia, codigo, cancellationToken);
            return response.ToResult();
        })
        .WithTags("Obrigacoes");

        return api;
    }
}
