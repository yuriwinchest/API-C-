using OneFlowApis.Infrastructure;
using OneFlowApis.Models;

namespace OneFlowApis.Endpoints;

public static class DiagnosticEndpoints
{
    public static RouteGroupBuilder MapDiagnosticEndpoints(this RouteGroupBuilder api)
    {
        var diagnostico = api.MapGroup(string.Empty).WithTags("Infra");

        diagnostico.MapGet("/configuracao/status", (
            OneFlowOptions oneFlowOptions,
            InternalApiSecurityOptions securityOptions,
            OneFlowResilienceOptions resilienceOptions,
            OneFlowAuthManager authManager) => Results.Ok(new
        {
            oneFlow = new
            {
                baseUrl = oneFlowOptions.OneFlowBaseUrl,
                tokenConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyToken),
                refreshTokenConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyRefreshToken),
                appHashConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyAppHash)
            },
            omie = new
            {
                portalAppsBaseUrl = oneFlowOptions.OmiePortalAppsBaseUrl,
                appKeyConfigurada = !string.IsNullOrWhiteSpace(oneFlowOptions.OmieAppKey),
                appSecretConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.OmieAppSecret)
            },
            autenticacaoInterna = new
            {
                cabecalho = securityOptions.HeaderName,
                chaveConfigurada = securityOptions.IsConfigured
            },
            resilienciaHttp = new
            {
                timeoutSegundos = resilienceOptions.TimeoutSeconds,
                retryCount = resilienceOptions.RetryCount,
                retryBaseDelayMs = resilienceOptions.RetryBaseDelayMs,
                circuitBreakerFailureThreshold = resilienceOptions.CircuitBreakerFailureThreshold,
                circuitBreakerBreakSeconds = resilienceOptions.CircuitBreakerBreakSeconds
            },
            gClick = new
            {
                baseUrl = oneFlowOptions.GClickBaseUrl,
                clientIdConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.GClickClientId),
                clientSecretConfigurado = !string.IsNullOrWhiteSpace(oneFlowOptions.GClickClientSecret)
            },
            prontoParaTesteOneFlow = !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyToken),
            prontoParaRenovacaoAutomatica =
                !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyToken) &&
                !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyRefreshToken) &&
                !string.IsNullOrWhiteSpace(oneFlowOptions.CompanyAppHash),
            prontoParaAutenticacaoInterna = securityOptions.IsConfigured,
            prontoParaGClick =
                !string.IsNullOrWhiteSpace(oneFlowOptions.GClickClientId) &&
                !string.IsNullOrWhiteSpace(oneFlowOptions.GClickClientSecret),
            autenticacao = authManager.GetDiagnostics()
        }));

        diagnostico.MapGet("/autenticacao/status", (OneFlowAuthManager authManager) =>
            Results.Ok(new
            {
                autenticacao = authManager.GetDiagnostics()
            }));

        diagnostico.MapPost("/autenticacao/refresh", async (
            OneFlowAuthManager authManager,
            CancellationToken cancellationToken) =>
        {
            await authManager.RefreshCompanyTokenAsync(cancellationToken, "endpoint-manual");

            return Results.Ok(new
            {
                mensagem = "Token do OneFlow renovado com sucesso.",
                autenticacao = authManager.GetDiagnostics()
            });
        });

        return api;
    }
}
