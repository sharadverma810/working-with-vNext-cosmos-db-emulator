using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Models;
using CosmosLearning.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Api.Controllers;

[ApiController]
[Route("products")]
public sealed class ProductsController(
    CosmosClient cosmosClient,
    IOptions<CosmosOptions> cosmosOptions,
    ILogger<ProductsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(ProductDocument product, CancellationToken cancellationToken)
    {
        if (!IsValidProduct(product))
        {
            return BadRequest(new { message = "The product id and category are required." });
        }

        try
        {
            var response = await Container.CreateItemAsync(
                product,
                new PartitionKey(product.Category),
                cancellationToken: cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id, category = product.Category },
                response.Resource);
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "create product");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "create product");
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        try
        {
            using var iterator = Container.GetItemQueryIterator<ProductDocument>(
                new QueryDefinition("SELECT * FROM c"),
                requestOptions: new QueryRequestOptions { MaxItemCount = 100 });

            var products = new List<ProductDocument>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                products.AddRange(response);
            }

            return Ok(new { items = products, count = products.Count });
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "list products");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "list products");
        }
    }

    [AcceptVerbs("QUERY")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("query")]
    [Consumes("application/json")]
    public async Task<IActionResult> Query(
        ProductQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (!ProductQueryBuilder.TryBuild(request, out var query, out var buildError))
        {
            return BadRequest(new { message = buildError });
        }

        try
        {
            using var iterator = Container.GetItemQueryIterator<ProductDocument>(
                query!,
                request.ContinuationToken,
                new QueryRequestOptions
                {
                    MaxItemCount = request.PageSize
                });

            var response = await iterator.ReadNextAsync(cancellationToken);
            var result = new ProductQueryResponse(
                response.ToList(),
                response.Count,
                response.ContinuationToken,
                response.RequestCharge,
                response.ActivityId);

            return Ok(result);
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "query products");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "query products");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id,
        [FromQuery] string category,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(category))
        {
            return BadRequest(new { message = "Both id and category are required for a point read." });
        }

        try
        {
            var response = await Container.ReadItemAsync<ProductDocument>(
                id,
                new PartitionKey(category),
                cancellationToken: cancellationToken);

            return Ok(response.Resource);
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "read product");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "read product");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Replace(
        string id,
        ProductDocument product,
        CancellationToken cancellationToken)
    {
        if (!IsValidProduct(product) || !string.Equals(id, product.Id, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "The route id must match the product id, and category is required." });
        }

        try
        {
            var response = await Container.ReplaceItemAsync(
                product,
                id,
                new PartitionKey(product.Category),
                cancellationToken: cancellationToken);

            return Ok(response.Resource);
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "replace product");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "replace product");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        [FromQuery] string category,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(category))
        {
            return BadRequest(new { message = "Both id and category are required for delete." });
        }

        try
        {
            await Container.DeleteItemAsync<ProductDocument>(
                id,
                new PartitionKey(category),
                cancellationToken: cancellationToken);

            return NoContent();
        }
        catch (CosmosException exception)
        {
            return HandleCosmosException(exception, "delete product");
        }
        catch (HttpRequestException exception)
        {
            return HandleTransportException(exception, "delete product");
        }
    }

    private Container Container => cosmosClient.GetContainer(
        cosmosOptions.Value.DatabaseName,
        cosmosOptions.Value.ContainerName);

    private static bool IsValidProduct(ProductDocument product) =>
        product is not null
        && !string.IsNullOrWhiteSpace(product.Id)
        && !string.IsNullOrWhiteSpace(product.Category);

    private IActionResult HandleCosmosException(CosmosException exception, string operation)
    {
        logger.LogError(
            exception,
            "Cosmos DB operation {Operation} failed with status code {StatusCode}.",
            operation,
            exception.StatusCode);

        if (exception.StatusCode == System.Net.HttpStatusCode.TooManyRequests
            && exception.RetryAfter is { } retryAfter
            && retryAfter > TimeSpan.Zero)
        {
            Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
        }

        var statusCode = exception.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => StatusCodes.Status400BadRequest,
            System.Net.HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
            System.Net.HttpStatusCode.Conflict => StatusCodes.Status409Conflict,
            System.Net.HttpStatusCode.TooManyRequests => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };

        return StatusCode(statusCode, new
        {
            message = statusCode == StatusCodes.Status500InternalServerError
                ? "The Cosmos DB operation failed."
                : $"The product operation could not be completed ({statusCode})."
        });
    }

    private IActionResult HandleTransportException(HttpRequestException exception, string operation)
    {
        logger.LogError(exception, "Cosmos DB transport operation {Operation} failed.", operation);

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            message = "The Cosmos DB emulator could not be reached. Check the endpoint and certificate."
        });
    }
}