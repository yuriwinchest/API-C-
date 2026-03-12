namespace OneFlowApis.Models;

public sealed class InternalApiSecurityOptions
{
    public const string AuthenticationScheme = "InternalApiKey";

    public string HeaderName { get; init; } = "X-Internal-Api-Key";
    public string? ApiKey { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !ApiKey.StartsWith("preencher_", StringComparison.OrdinalIgnoreCase);

    public static InternalApiSecurityOptions FromConfiguration(IConfiguration configuration)
    {
        return new InternalApiSecurityOptions
        {
            HeaderName = configuration["INTERNAL_API_KEY_HEADER_NAME"] ?? "X-Internal-Api-Key",
            ApiKey = configuration["INTERNAL_API_KEY"]
        };
    }
}
