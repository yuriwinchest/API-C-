namespace OneFlowApis.Models;

public sealed class OneFlowOptions
{
    public int Port { get; init; } = 3000;
    public string OneFlowBaseUrl { get; init; } = "https://rest.oneflow.com.br/api";
    public string OmiePortalAppsBaseUrl { get; init; } = "https://app.omie.com.br/api/portal/apps";
    public string? CompanyToken { get; init; }
    public string? CompanyRefreshToken { get; init; }
    public string? CompanyAppHash { get; init; }

    public static OneFlowOptions FromConfiguration(IConfiguration configuration)
    {
        var port = 3000;
        _ = int.TryParse(configuration["PORT"], out port);

        return new OneFlowOptions
        {
            Port = port <= 0 ? 3000 : port,
            OneFlowBaseUrl = configuration["ONEFLOW_BASE_URL"] ?? "https://rest.oneflow.com.br/api",
            OmiePortalAppsBaseUrl = configuration["OMIE_PORTAL_APPS_BASE_URL"] ?? "https://app.omie.com.br/api/portal/apps",
            CompanyToken = configuration["ONEFLOW_COMPANY_TOKEN"],
            CompanyRefreshToken = configuration["ONEFLOW_COMPANY_REFRESH_TOKEN"],
            CompanyAppHash = configuration["ONEFLOW_COMPANY_APP_HASH"]
        };
    }
}
