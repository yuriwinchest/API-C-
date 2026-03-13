using OneFlowApis.Services;

namespace OneFlowApis.Endpoints;

public static class EmpresaEndpoints
{
    public static RouteGroupBuilder MapEmpresaEndpoints(this RouteGroupBuilder api)
    {
        var empresa = api.MapGroup("/empresa").WithTags("Empresa");

        empresa.MapGet("/dados-basicos", async (OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetEmpresaDadosBasicosAsync(cancellationToken);
            return response.ToResult();
        });

        empresa.MapGet("/dadosbasicos", async (OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetEmpresaDadosBasicosAsync(cancellationToken);
            return response.ToResult();
        });

        return api;
    }
}
