using CosmosLearning.Vector.Api.Features.VectorSearch;
using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Vector.Api.Controllers;

[ApiController]
[Route("api/vector")]
public sealed class VectorProductsController : ControllerBase
{
    private readonly CosmosVectorRepository _repository;
    private readonly VectorSearchService _searchService;

    public VectorProductsController(
        CosmosVectorRepository repository,
        VectorSearchService searchService)
    {
        _repository = repository;
        _searchService = searchService;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(
        CancellationToken cancellationToken)
    {
        int productCount =
            await _repository.GetProductCountAsync(
                cancellationToken);

        return Ok(new
        {
            database = "VectorDemoDb",
            container = "Products",
            products = productCount,
            status = "Ready"
        });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(
        CancellationToken cancellationToken)
    {
        int inserted =
            await _repository.SeedProductsAsync(
                cancellationToken);

        return Ok(new
        {
            inserted,
            message = inserted == 0
                ? "Data already exists."
                : "Products were embedded with Ollama and stored in Cosmos DB."
        });
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<VectorSearchResult>>> Search(
        [FromBody] VectorSearchRequest request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<VectorSearchResult> results =
            await _searchService.SearchAsync(
                request,
                cancellationToken);

        return Ok(new
        {
            query = request.Query,
            results
        });
    }
}