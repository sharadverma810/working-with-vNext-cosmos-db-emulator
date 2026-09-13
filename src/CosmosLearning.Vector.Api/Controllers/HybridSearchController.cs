using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

[ApiController]
[Route("api/hybrid")]
public sealed class HybridSearchController : ControllerBase
{
    private readonly HybridSearchService _searchService;
    private readonly CosmosHybridRepository _repository;

    public HybridSearchController(
        HybridSearchService searchService,
        CosmosHybridRepository repository)
    {
        _searchService = searchService;
        _repository = repository;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        var results = await _searchService.SearchAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            query = request.Query,
            mode = request.Mode,
            results
        });
    }
}