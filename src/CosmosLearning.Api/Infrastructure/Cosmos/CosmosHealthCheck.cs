using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Infrastructure.Cosmos;

public sealed class CosmosHealthCheck(
    CosmosClient cosmosClient,
    ILogger<CosmosHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cosmosClient.ReadAccountAsync();

            return HealthCheckResult.Healthy(
                "Cosmos DB is available.");
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Cosmos DB health check failed.");

            return HealthCheckResult.Unhealthy(
                "Cosmos DB is unavailable.",
                exception);
        }
    }
}