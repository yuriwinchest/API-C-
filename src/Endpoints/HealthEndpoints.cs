namespace OneFlowApis.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new
        {
            status = "ok",
            servico = "one-flow-apis"
        }))
        .WithTags("Infra");

        return endpoints;
    }
}
