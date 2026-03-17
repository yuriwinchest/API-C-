namespace OneFlowApis.Infrastructure;

public sealed class DotEnvOneFlowCredentialStore : IOneFlowCredentialStore
{
    private readonly ILogger<DotEnvOneFlowCredentialStore> _logger;
    private readonly string _dotEnvPath;
    private readonly object _sync = new();

    private bool? _lastPersistSucceeded;
    private DateTimeOffset? _lastPersistedAtUtc;
    private string? _lastPersistMessage;

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
        var persistedAtUtc = DateTimeOffset.UtcNow;
        Environment.SetEnvironmentVariable("ONEFLOW_COMPANY_TOKEN", token);
        Environment.SetEnvironmentVariable("ONEFLOW_COMPANY_REFRESH_TOKEN", refreshToken);

        if (!File.Exists(_dotEnvPath))
        {
            RecordPersistResult(
                false,
                persistedAtUtc,
                "Arquivo .env nao encontrado. Credenciais renovadas ficaram apenas em memoria.");

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
            RecordPersistResult(
                true,
                persistedAtUtc,
                "Credenciais renovadas persistidas com sucesso no arquivo .env.");

            _logger.LogInformation(
                "Credenciais renovadas do OneFlow persistidas em {Path}.",
                _dotEnvPath);
        }
        catch (Exception exception)
        {
            RecordPersistResult(
                false,
                persistedAtUtc,
                "Falha ao persistir o .env. Credenciais renovadas mantidas apenas em memoria.");

            _logger.LogWarning(
                exception,
                "Falha ao persistir as credenciais renovadas do OneFlow em {Path}. A renovacao continuara valida em memoria ate o proximo reinicio.",
                _dotEnvPath);
        }
    }

    public OneFlowCredentialStoreDiagnostics GetDiagnostics()
    {
        var envFileExists = File.Exists(_dotEnvPath);
        var envFileWritable = envFileExists && !new FileInfo(_dotEnvPath).IsReadOnly;
        var mode = envFileWritable ? "memoria-e-arquivo" : "somente-memoria";

        lock (_sync)
        {
            return new OneFlowCredentialStoreDiagnostics(
                mode,
                envFileExists,
                envFileWritable,
                _lastPersistSucceeded,
                _lastPersistedAtUtc,
                _lastPersistMessage);
        }
    }

    private void RecordPersistResult(bool succeeded, DateTimeOffset persistedAtUtc, string message)
    {
        lock (_sync)
        {
            _lastPersistSucceeded = succeeded;
            _lastPersistedAtUtc = persistedAtUtc;
            _lastPersistMessage = message;
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
