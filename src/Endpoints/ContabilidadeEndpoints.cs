using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OneFlowApis.Services;
using OneFlowApis.Validation;

namespace OneFlowApis.Endpoints;

public static class ContabilidadeEndpoints
{
    public static RouteGroupBuilder MapContabilidadeEndpoints(this RouteGroupBuilder api)
    {
        var contabil = api.MapGroup("/contabilidade").WithTags("Contabilidade");

        contabil.MapPost("/lancamentos", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentoscontabeis", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentos/transacao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilTransacaoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentoscontabeis/transacao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilTransacaoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentos/padrao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilPadraoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentoscontabeis/padrao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostLancamentoContabilPadraoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentos/excluir", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapDelete("/lancamentoscontabeis/id", async ([FromBody] JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentos/excluir-transacao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilTransacaoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapDelete("/lancamentoscontabeis/transacao", async ([FromBody] JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilTransacaoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapPost("/lancamentos/excluir-padrao", async (JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilPadraoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapDelete("/lancamentoscontabeis/padrao", async ([FromBody] JsonElement body, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var response = await service.PostExcluirLancamentoContabilPadraoAsync(body, cancellationToken);
            return response.ToResult();
        });

        contabil.MapGet("/plano-contas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var id = RequestValidators.OptionalPositiveInt(request.Query["id"], "id");
            var pagina = RequestValidators.OptionalPositiveInt(request.Query["pagina"], "pagina");
            RequestValidators.RequireAtLeastOne(id, pagina, "id", "pagina");

            var response = await service.GetPlanoContasAsync(id, pagina, cancellationToken);
            return response.ToResult();
        });

        contabil.MapGet("/planocontas", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var id = RequestValidators.OptionalPositiveInt(request.Query["id"], "id");
            var pagina = RequestValidators.OptionalPositiveInt(request.Query["pagina"], "pagina");
            RequestValidators.RequireAtLeastOne(id, pagina, "id", "pagina");

            var response = await service.GetPlanoContasAsync(id, pagina, cancellationToken);
            return response.ToResult();
        });

        contabil.MapGet("/balancete", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
            var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");
            var zeramento = RequestValidators.RequiredAllowedValue(request.Query["zeramento"], "zeramento", ["S", "N"]);

            var response = await service.GetBalanceteAsync(competenciaInicial, competenciaFinal, zeramento, cancellationToken);
            return response.ToResult();
        });

        contabil.MapGet("/razao", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var conta = RequestValidators.RequiredString(request.Query["conta"], "conta");
            var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
            var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");

            var response = await service.GetRazaoAsync(conta, competenciaInicial, competenciaFinal, cancellationToken);
            return response.ToResult();
        });

        contabil.MapGet("/lancamentos/conta", async (HttpRequest request, OneFlowService service, CancellationToken cancellationToken) =>
        {
            var conta = RequestValidators.RequiredString(request.Query["conta"], "conta");
            var competenciaInicial = RequestValidators.RequiredCompetencia(request.Query["competenciaInicial"], "competenciaInicial");
            var competenciaFinal = RequestValidators.RequiredCompetencia(request.Query["competenciaFinal"], "competenciaFinal");

            var response = await service.GetRazaoAsync(conta, competenciaInicial, competenciaFinal, cancellationToken);
            return response.ToResult();
        });

        return api;
    }
}
