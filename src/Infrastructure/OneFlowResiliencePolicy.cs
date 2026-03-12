using OneFlowApis.Models;

namespace OneFlowApis.Infrastructure;

public sealed class OneFlowResiliencePolicy
{
    private readonly OneFlowResilienceOptions _options;
    private readonly ILogger<OneFlowResiliencePolicy> _logger;
    private readonly object _sync = new();

    private int _consecutiveFailures;
    private DateTimeOffset? _circuitOpenUntil;

    public OneFlowResiliencePolicy(
        OneFlowResilienceOptions options,
        ILogger<OneFlowResiliencePolicy> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        HttpMethod method,
        string path,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        EnsureCircuitClosed(path);

        var maxAttempts = IsRetryEnabled(method) ? _options.RetryCount + 1 : 1;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            try
            {
                var result = await operation(timeoutCts.Token);
                RegisterSuccess();
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
            {
                var timeoutException = new AppException(
                    504,
                    "Tempo limite excedido ao comunicar com a API do OneFlow.",
                    new
                    {
                        path,
                        tentativa = attempt,
                        timeoutSegundos = _options.TimeoutSeconds
                    });

                if (attempt >= maxAttempts || !IsRetryEnabled(method))
                {
                    RegisterFailure(path);
                    throw timeoutException;
                }

                RegisterFailure(path);
                await DelayBeforeRetryAsync(path, attempt, cancellationToken);
            }
            catch (AppException exception) when (IsTransientStatusCode(exception.StatusCode) && attempt < maxAttempts && IsRetryEnabled(method))
            {
                RegisterFailure(path);
                await DelayBeforeRetryAsync(path, attempt, cancellationToken);
            }
            catch (AppException exception) when (IsTransientStatusCode(exception.StatusCode))
            {
                RegisterFailure(path);
                throw;
            }
        }

        throw new AppException(502, "Falha inesperada na resiliencia da comunicacao com o OneFlow.");
    }

    private void EnsureCircuitClosed(string path)
    {
        lock (_sync)
        {
            if (!_circuitOpenUntil.HasValue)
            {
                return;
            }

            if (_circuitOpenUntil.Value <= DateTimeOffset.UtcNow)
            {
                _circuitOpenUntil = null;
                _consecutiveFailures = 0;
                return;
            }

            throw new AppException(
                503,
                "Circuit breaker aberto para a comunicacao com o OneFlow.",
                new
                {
                    path,
                    reabrirEm = _circuitOpenUntil.Value
                });
        }
    }

    private void RegisterSuccess()
    {
        lock (_sync)
        {
            _consecutiveFailures = 0;
            _circuitOpenUntil = null;
        }
    }

    private void RegisterFailure(string path)
    {
        lock (_sync)
        {
            _consecutiveFailures++;

            if (_consecutiveFailures < _options.CircuitBreakerFailureThreshold)
            {
                return;
            }

            _circuitOpenUntil = DateTimeOffset.UtcNow.AddSeconds(_options.CircuitBreakerBreakSeconds);
            _logger.LogWarning(
                "Circuit breaker aberto para {Path} apos {FailureCount} falhas consecutivas.",
                path,
                _consecutiveFailures);
        }
    }

    private async Task DelayBeforeRetryAsync(string path, int attempt, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMilliseconds(_options.RetryBaseDelayMs * attempt);

        _logger.LogWarning(
            "Nova tentativa da chamada OneFlow para {Path}. Tentativa atual: {Attempt}. Aguardando {DelayMs} ms.",
            path,
            attempt + 1,
            delay.TotalMilliseconds);

        await Task.Delay(delay, cancellationToken);
    }

    private static bool IsRetryEnabled(HttpMethod method)
    {
        return method == HttpMethod.Get || method == HttpMethod.Head || method == HttpMethod.Options;
    }

    private static bool IsTransientStatusCode(int statusCode)
    {
        return statusCode == 408 || statusCode == 429 || statusCode >= 500;
    }
}
