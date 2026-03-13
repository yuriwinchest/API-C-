using OneFlowApis.Services;
using OneFlowApis.Validation;

namespace OneFlowApis.Endpoints;

public static class EscritorioEndpoints
{
    public static RouteGroupBuilder MapEscritorioEndpoints(this RouteGroupBuilder api)
    {
        var escritorio = api.MapGroup("/escritorio").WithTags("Escritorio");

        escritorio.MapGet("/empresas/listar", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var pagina = RequestValidators.RequiredPositiveInt(request.Query["pagina"], "pagina");
            var response = await service.GetEscritorioEmpresasListarAsync(pagina, cancellationToken);
            return response.ToResult();
        });

        escritorio.MapGet("/empresas/detalhes", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var cnpj = RequestValidators.RequiredCnpj(request.Query["cnpj"], "cnpj");
            var response = await service.GetEscritorioEmpresasDetalhesAsync(cnpj, cancellationToken);
            return response.ToResult();
        });

        return api;
    }
}
