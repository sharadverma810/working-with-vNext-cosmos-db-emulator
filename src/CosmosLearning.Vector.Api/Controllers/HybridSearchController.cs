using CosmosLearning.Vector.Api.Features.HybridSearch;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Vector.Api.Controllers;

[ApiController]
[Route("api/hybrid")]
public sealed class HybridSearchController : ControllerBase
{
    private readonly HybridSearchService _service;

    public HybridSearchController(
        HybridSearchService service)
    {
        _service = service;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(
        [FromQuery] int count = 1000,
        CancellationToken cancellationToken = default)
    {
        int inserted =
            await _service.SeedAsync(
                count,
                cancellationToken);

        return Ok(new
        {
            requested = count,
            inserted,
            message =
                "Hybrid search catalog has been embedded with Ollama and stored in Cosmos DB."
        });
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        var results =
            await _service.SearchAsync(
                request,
                cancellationToken);

        return Ok(new
        {
            query = request.Query,
            results
        });
    }
}