using CosmosLearning.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Api.Controllers;

[ApiController]
[Route("health/cosmos")]
public sealed class CosmosHealthController(
    CosmosClient cosmosClient,
    IOptions<CosmosOptions> cosmosOptions,
    ILogger<CosmosHealthController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await cosmosClient.ReadAccountAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return Ok(new
            {
                status = "Healthy",
                endpoint = cosmosOptions.Value.Endpoint,
                database = cosmosOptions.Value.DatabaseName,
                container = cosmosOptions.Value.ContainerName
            });
        }
        catch (CosmosException exception)
        {
            logger.LogError(
                exception,
                "Cosmos DB connectivity check failed with status code {StatusCode}.",
                exception.StatusCode);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                message = "The Cosmos DB emulator could not be reached."
            });
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "The Cosmos DB connectivity check could not establish a secure connection.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                message = "The Cosmos DB emulator could not be reached. Check the emulator certificate and endpoint."
            });
        }
    }
}