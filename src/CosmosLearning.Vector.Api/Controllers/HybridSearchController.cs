using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

[ApiController]
[Route("api/hybrid")]
public sealed class HybridSearchController : ControllerBase
{
    private readonly HybridSearchService _service;
    private readonly CosmosHybridRepository _repository;

    public HybridSearchController(
        HybridSearchService service,
        CosmosHybridRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    // ============================================================
    // POST: /api/hybrid/seed?count=1000
    // ============================================================

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(
        [FromQuery] int count = 1000,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return BadRequest(
                new
                {
                    message = "Count must be greater than zero."
                });
        }

        int inserted =
            await _repository.SeedAsync(
                count,
                cancellationToken);

        return Ok(
            new
            {
                requested = count,
                inserted,
                message =
                    "Hybrid search catalog has been embedded with Ollama and stored in Cosmos DB."
            });
    }

    // ============================================================
    // DELETE: /api/hybrid/reset
    // ============================================================

    [HttpDelete("reset")]
    public async Task<IActionResult> Reset(
        CancellationToken cancellationToken = default)
    {
        int deleted =
            await _repository.DeleteAllAsync(
                cancellationToken);

        return Ok(
            new
            {
                deleted,
                message =
                    "Hybrid search catalog has been cleared."
            });
    }

    // ============================================================
    // POST: /api/hybrid/search
    // ============================================================

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] HybridSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var results =
            await _service.SearchAsync(
                request,
                cancellationToken);

        var mode =
      string.IsNullOrWhiteSpace(request.Mode)
          ? "hybrid"
          : request.Mode.Trim().ToLowerInvariant();

        return Ok(new
        {
            query = request.Query,

            mode,

            vectorWeight = request.VectorWeight,

            keywordWeight = request.KeywordWeight,

            top = request.Top,

            searchExplanation = new
            {
                algorithm = "Weighted Reciprocal Rank Fusion",

                rankConstant =
                    ReciprocalRankFusion.DefaultRankConstant,

                vectorCandidateCount =
                    HybridSearchService.VectorCandidateCount,

                vectorWeight =
                    request.VectorWeight,

                keywordWeight =
                    request.KeywordWeight,

                vectorSearch =
                    "Cosmos DB VectorDistance with DiskANN",

                keywordSearch =
                    "Application-side BM25-style lexical ranking",

                fusion =
                    "Weighted RRF combines vector and keyword ranks"
            },

            results
        });
    }
}