using CosmosLearning.Api.Features.Products.Pagination.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Features.Products.Pagination;

[ApiController]
[Route("products/pages")]
public sealed class ProductPaginationController(ProductPaginationService paginationService, ILogger<ProductPaginationController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ProductPageResponse), StatusCodes.Status200OK)]

    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPage(
    [FromQuery] int pageSize,
    [FromQuery] string? category,
    [FromQuery] bool? isActive,
    [FromQuery] decimal? minimumPrice,
    [FromQuery] decimal? maximumPrice,

    [FromHeader(Name = "x-continuation-token")]
        string? continuationToken,

    CancellationToken cancellationToken)
    {
        var request = new ProductPageRequest
        {
            PageSize = pageSize,

            Category = category,

            IsActive = isActive,

            MinimumPrice = minimumPrice,

            MaximumPrice = maximumPrice,

            ContinuationToken = continuationToken
        };

        if (!ProductPaginationValidator.TryValidate(request, out var validationError))
        {
            return BadRequest(new
            {
                message = validationError
            });
        }

        try
        {
            var result =
                await paginationService.GetPageAsync(request, cancellationToken);

            return Ok(result);
        }
        catch (CosmosException exception)
        {
            logger.LogError(
                exception,
                """
                Cosmos DB pagination operation failed.
                StatusCode: {StatusCode}.
                """,
                exception.StatusCode);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "The Cosmos DB pagination operation failed."
                });
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                """
                Cosmos DB could not be reached
                during pagination.
                """);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        """
                        The Cosmos DB emulator could not
                        be reached.
                        Check the endpoint and certificate.
                        """
                });
        }
    }
}