using System.Text.Json;
using OneFlowApis.Services;
using OneFlowApis.Validation;

namespace OneFlowApis.Endpoints;

public static class FiscalEndpoints
{
    public static RouteGroupBuilder MapFiscalEndpoints(this RouteGroupBuilder api)
    {
        var fiscal = api.MapGroup("/fiscal").WithTags("Fiscal");

        fiscal.MapGet("/documentos/totais", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosTotaisAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/total/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosTotaisAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/documentos/quantidade", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosQuantidadeAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/documentos/listar", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosListarAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/documentos/por-socio", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosPorSocioAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/documentos/total/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetDocumentosListarAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/apuracoes", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetApuracoesFiscaisAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/apuracoes/resumo", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var imposto = RequestValidators.RequiredString(request.Query["imposto"], "imposto");
            var response = await service.GetResumoApuracaoFiscalAsync(competencia, imposto, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/apuracao/competencia/imposto", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetApuracoesFiscaisAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/simples-nacional/aliquotas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetAliquotasSimplesNacionalAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapGet("/aliquotas/simplesnacional/competencia", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competencia = RequestValidators.RequiredCompetencia(request.Query["competencia"], "competencia");
            var response = await service.GetAliquotasSimplesNacionalAsync(competencia, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfse/nacional", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalNfseNacionalAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfsenacional/incluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalNfseNacionalAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfse/prefeitura", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalNfsePrefeituraAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfse/layout-oneflow", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalNfseLayoutOneFlowAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfseprefeitura/incluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalNfsePrefeituraAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/documentos/remover", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalDocumentoRemoverAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfse/remove", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalDocumentoRemoverAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/documentos/alterar-status", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalDocumentoAlterarStatusAsync(body, cancellationToken);
            return response.ToResult();
        });

        fiscal.MapPost("/nfsestatus/alterar", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostFiscalDocumentoAlterarStatusAsync(body, cancellationToken);
            return response.ToResult();
        });

        return api;
    }
}
