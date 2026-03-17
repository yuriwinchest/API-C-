namespace OneFlowApis.Infrastructure;

public sealed class DotEnvOneFlowCredentialStore : IOneFlowCredentialStore
{
    private readonly ILogger<DotEnvOneFlowCredentialStore> _logger;
    private readonly string _dotEnvPath;

    public DotEnvOneFlowCredentialStore(ILogger<DotEnvOneFlowCredentialStore> logger)
        : this(logger, Path.Combine(Directory.GetCurrentDirectory(), ".env"))
    {
    }

    public DotEnvOneFlowCredentialStore(ILogger<DotEnvOneFlowCredentialStore> logger, string dotEnvPath)
    {
        _logger = logger;
        _dotEnvPath = dotEnvPath;
    }

    public void PersistRefreshedCredentials(string token, string refreshToken)
    {
        Environment.SetEnvironmentVariable("ONEFLOW_COMPANY_TOKEN", token);
        Environment.SetEnvironmentVariable("ONEFLOW_COMPANY_REFRESH_TOKEN", refreshToken);

        if (!File.Exists(_dotEnvPath))
        {
            _logger.LogWarning(
                "Arquivo .env nao encontrado em {Path}. As credenciais renovadas ficaram apenas em memoria.",
                _dotEnvPath);
            return;
        }

        try
        {
            var lines = File.ReadAllLines(_dotEnvPath).ToList();

            Upsert(lines, "ONEFLOW_COMPANY_TOKEN", token);
            Upsert(lines, "ONEFLOW_COMPANY_REFRESH_TOKEN", refreshToken);

            File.WriteAllLines(_dotEnvPath, lines);

            _logger.LogInformation(
                "Credenciais renovadas do OneFlow persistidas em {Path}.",
                _dotEnvPath);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falha ao persistir as credenciais renovadas do OneFlow em {Path}. A renovacao continuara valida em memoria ate o proximo reinicio.",
                _dotEnvPath);
        }
    }

    private static void Upsert(List<string> lines, string key, string value)
    {
        var prefix = $"{key}=";
        var index = lines.FindIndex(line => line.TrimStart().StartsWith(prefix, StringComparison.Ordinal));

        if (index >= 0)
        {
            lines[index] = $"{key}={value}";
            return;
        }

        lines.Add($"{key}={value}");
    }
}
