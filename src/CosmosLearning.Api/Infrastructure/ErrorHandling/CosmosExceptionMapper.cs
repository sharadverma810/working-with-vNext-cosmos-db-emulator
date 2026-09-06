using System.Net;
using CosmosLearning.Api.Features.ErrorHandling.Exceptions;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Infrastructure.ErrorHandling;

public static class CosmosExceptionMapper
{
    public static Exception Map(
        CosmosException exception,
        string operation)
    {
        return exception.StatusCode switch
        {
            HttpStatusCode.NotFound =>
                new CosmosResourceNotFoundException(
                    $"The requested Cosmos DB resource was not found while attempting to {operation}.",
                    exception),

            HttpStatusCode.Conflict =>
                new CosmosConflictException(
                    $"A conflict occurred while attempting to {operation}.",
                    exception),

            HttpStatusCode.TooManyRequests =>
                new CosmosThrottlingException(
                    $"Cosmos DB throttled the request while attempting to {operation}.",
                    exception.RetryAfter ?? TimeSpan.FromSeconds(1),
                    exception),

            HttpStatusCode.ServiceUnavailable =>
                new CosmosUnavailableException(
                    $"Cosmos DB is temporarily unavailable while attempting to {operation}.",
                    exception),

            _ => exception
        };
    }
}