namespace OneFlowApis.Models;

public sealed class OneFlowResilienceOptions
{
    public int TimeoutSeconds { get; init; } = 30;
    public int RetryCount { get; init; } = 2;
    public int RetryBaseDelayMs { get; init; } = 500;
    public int CircuitBreakerFailureThreshold { get; init; } = 5;
    public int CircuitBreakerBreakSeconds { get; init; } = 30;

    public static OneFlowResilienceOptions FromConfiguration(IConfiguration configuration)
    {
        return new OneFlowResilienceOptions
        {
            TimeoutSeconds = ReadPositiveInt(configuration["ONEFLOW_HTTP_TIMEOUT_SECONDS"], 30),
            RetryCount = ReadNonNegativeInt(configuration["ONEFLOW_HTTP_RETRY_COUNT"], 2),
            RetryBaseDelayMs = ReadPositiveInt(configuration["ONEFLOW_HTTP_RETRY_BASE_DELAY_MS"], 500),
            CircuitBreakerFailureThreshold = ReadPositiveInt(configuration["ONEFLOW_HTTP_CIRCUIT_BREAKER_FAILURE_THRESHOLD"], 5),
            CircuitBreakerBreakSeconds = ReadPositiveInt(configuration["ONEFLOW_HTTP_CIRCUIT_BREAKER_BREAK_SECONDS"], 30)
        };
    }

    private static int ReadPositiveInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    private static int ReadNonNegativeInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) && parsedValue >= 0
            ? parsedValue
            : defaultValue;
    }
}
