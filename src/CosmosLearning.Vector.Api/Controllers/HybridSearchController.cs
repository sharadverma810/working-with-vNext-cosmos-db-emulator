using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

[ApiController]
[Route("api/hybrid")]
public sealed class HybridSearchController : ControllerBase
{
    private readonly HybridSearchService _searchService;

    public HybridSearchController(HybridSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
    [FromBody] HybridSearchRequest request,
    CancellationToken cancellationToken)
    {
        var results =
            await _searchService.SearchAsync(
                request,
                cancellationToken);

        return Ok(new
        {
            query = request.Query,

            mode = string.IsNullOrWhiteSpace(request.Mode)
                ? "hybrid"
                : request.Mode,

            vectorWeight = request.VectorWeight,

            keywordWeight = request.KeywordWeight,

            top = request.Top,

            results
        });
    }
}