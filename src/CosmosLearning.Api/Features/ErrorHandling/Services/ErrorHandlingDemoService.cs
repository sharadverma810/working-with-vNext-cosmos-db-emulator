using System.Net;
using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.ErrorHandling.Exceptions;
using CosmosLearning.Api.Features.ErrorHandling.Models;
using CosmosLearning.Api.Infrastructure.ErrorHandling;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Api.Features.ErrorHandling.Services;

public sealed class ErrorHandlingDemoService
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosOptions _cosmosOptions;
    private readonly ILogger<ErrorHandlingDemoService> _logger;

    public ErrorHandlingDemoService(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        ILogger<ErrorHandlingDemoService> logger)
    {
        _cosmosClient = cosmosClient;
        _cosmosOptions = cosmosOptions.Value;
        _logger = logger;
    }

    private Container Container =>
        _cosmosClient.GetContainer(
            _cosmosOptions.DatabaseName,
            _cosmosOptions.ContainerName);

    public Task<ErrorHandlingDemoResponse> SuccessAsync(
        string traceId)
    {
        return Task.FromResult(
            new ErrorHandlingDemoResponse(
                "The request completed successfully.",
                traceId,
                DateTimeOffset.UtcNow));
    }

    public async Task SimulateNotFoundAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await Container.ReadItemAsync<object>(
                "does-not-exist",
                new PartitionKey("does-not-exist"),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException exception)
        {
            throw CosmosExceptionMapper.Map(
                exception,
                "read a product");
        }
    }

    public async Task SimulateConflictAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var document = new
            {
                id = "error-handling-conflict-demo",
                category = "ErrorHandling",
                name = "Conflict demonstration"
            };

            await Container.CreateItemAsync(
                document,
                new PartitionKey("ErrorHandling"),
                cancellationToken: cancellationToken);

            await Container.CreateItemAsync(
                document,
                new PartitionKey("ErrorHandling"),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException exception)
        {
            throw CosmosExceptionMapper.Map(
                exception,
                "create a duplicate product");
        }
    }

    public Task SimulateThrottlingAsync()
    {
        throw new CosmosThrottlingException(
            "This endpoint simulates a Cosmos DB throttling response.",
            TimeSpan.FromSeconds(5));
    }

    public Task SimulateUnavailableAsync()
    {
        throw new CosmosUnavailableException(
            "This endpoint simulates a temporary Cosmos DB availability problem.");
    }

    public Task SimulateUnexpectedFailureAsync()
    {
        throw new InvalidOperationException(
            "This endpoint intentionally demonstrates an unexpected application failure.");
    }

    public async Task SimulateCancellationAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting a long-running operation.");

        await Task.Delay(
            TimeSpan.FromSeconds(30),
            cancellationToken);
    }
}