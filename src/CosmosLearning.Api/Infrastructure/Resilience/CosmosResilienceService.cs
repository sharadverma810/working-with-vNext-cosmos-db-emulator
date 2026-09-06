using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Infrastructure.Resilience;

public sealed class CosmosResilienceService(
    ILogger<CosmosResilienceService> logger)
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 3;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (CosmosException exception)
                when (IsTransient(exception) && attempt <= maxRetries)
            {
                var delay = GetRetryDelay(exception, attempt);

                logger.LogWarning(
                    exception,
                    """
                    Cosmos DB transient failure.

                    Operation: {Operation}
                    Attempt: {Attempt}
                    StatusCode: {StatusCode}
                    RetryDelay: {RetryDelay}
                    """,
                    operationName,
                    attempt,
                    exception.StatusCode,
                    delay);

                await Task.Delay(delay, cancellationToken);
            }
            catch (HttpRequestException exception)
                when (attempt <= maxRetries)
            {
                var delay =
                    TimeSpan.FromMilliseconds(500 * attempt);

                logger.LogWarning(
                    exception,
                    """
                    Cosmos DB network failure.

                    Operation: {Operation}
                    Attempt: {Attempt}
                    RetryDelay: {RetryDelay}
                    """,
                    operationName,
                    attempt,
                    delay);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(CosmosException exception)
    {
        return exception.StatusCode is
            System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.InternalServerError
            or System.Net.HttpStatusCode.ServiceUnavailable;
    }

    private static TimeSpan GetRetryDelay(
        CosmosException exception,
        int attempt)
    {
        if (exception.StatusCode ==
                System.Net.HttpStatusCode.TooManyRequests
            && exception.RetryAfter is { } retryAfter
            && retryAfter > TimeSpan.Zero)
        {
            return retryAfter;
        }

        var milliseconds =
            500 * Math.Pow(2, attempt - 1);

        return TimeSpan.FromMilliseconds(milliseconds);
    }
}