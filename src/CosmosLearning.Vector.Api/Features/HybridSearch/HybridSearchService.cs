using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchService
{
    private readonly CosmosHybridRepository _repository;
    private readonly KeywordSearchService _keywordSearchService;
    public const int VectorCandidateCount = 100;

    public HybridSearchService(
        CosmosHybridRepository repository,
        KeywordSearchService keywordSearchService)
    {
        _repository = repository;
        _keywordSearchService = keywordSearchService;
    }

    public async Task<IReadOnlyList<HybridSearchResult>> SearchAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "Query is required.",
                nameof(request.Query));
        }

        if (request.Top <= 0)
        {
            request.Top = 5;
        }

        if (request.VectorWeight < 0)
        {
            throw new ArgumentException(
                "VectorWeight cannot be negative.",
                nameof(request.VectorWeight));
        }

        if (request.KeywordWeight < 0)
        {
            throw new ArgumentException(
                "KeywordWeight cannot be negative.",
                nameof(request.KeywordWeight));
        }

        if (request.VectorWeight == 0 &&
            request.KeywordWeight == 0)
        {
            throw new ArgumentException(
                "At least one search weight must be greater than zero.");
        }

        var mode = ParseMode(request.Mode);

        return mode switch
        {
            HybridSearchMode.Vector =>
                await SearchVectorAsync(
                    request,
                    cancellationToken),

            HybridSearchMode.Keyword =>
                await SearchKeywordAsync(
                    request,
                    cancellationToken),

            HybridSearchMode.Hybrid =>
                await SearchHybridAsync(
                    request,
                    cancellationToken),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<IReadOnlyList<HybridSearchResult>> SearchVectorAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        var candidates =
            await _repository.VectorSearchAsync(
                request,
                request.Top,
                cancellationToken);

        return candidates
            .Take(request.Top)
            .Select(
                (candidate, index) =>
                    new HybridSearchResult
                    {
                        Id = candidate.Id,
                        Name = candidate.Name,
                        Category = candidate.Category,
                        Price = candidate.Price,
                        Description = candidate.Description,

                        VectorRank = index + 1,
                        KeywordRank = null,

                        RrfScore = 0
                    })
            .ToList();
    }

    private async Task<IReadOnlyList<HybridSearchResult>> SearchKeywordAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        var candidates =
            await _repository.GetKeywordCandidatesAsync(
                request,
                cancellationToken);

        var keywordResults =
            _keywordSearchService.Search(
                request.Query,
                candidates);

        return keywordResults
            .Take(request.Top)
            .Select(
                (result, index) =>
                    new HybridSearchResult
                    {
                        Id = result.Document.Id,
                        Name = result.Document.Name,
                        Category = result.Document.Category,
                        Price = result.Document.Price,
                        Description = result.Document.Description,

                        VectorRank = null,
                        KeywordRank = index + 1,

                        RrfScore = 0
                    })
            .ToList();
    }

    private async Task<IReadOnlyList<HybridSearchResult>> SearchHybridAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        /*
         * We retrieve a larger vector candidate pool.
         *
         * Top = 5
         * Vector candidates = 100
         *
         * This allows documents that are not in the first
         * five vector results to still participate in RRF.
         */
        // const int vectorCandidateCount = VectorCandidateCount;

        var vectorCandidates =
     await _repository.VectorSearchAsync(
         request,
         VectorCandidateCount,
         cancellationToken);

        var keywordCandidates =
            await _repository.GetKeywordCandidatesAsync(
                request,
                cancellationToken);

        var keywordResults =
            _keywordSearchService.Search(
                request.Query,
                keywordCandidates);

        return ReciprocalRankFusion.Fuse(
                vectorCandidates,
                keywordResults,
                request.Top,
                request.VectorWeight,
                request.KeywordWeight)
            .ToList();
    }

    private static HybridSearchMode ParseMode(
        string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return HybridSearchMode.Hybrid;
        }

        return mode.Trim().ToLowerInvariant() switch
        {
            "vector" =>
                HybridSearchMode.Vector,

            "keyword" =>
                HybridSearchMode.Keyword,

            "hybrid" =>
                HybridSearchMode.Hybrid,

            _ => throw new ArgumentException(
                "Invalid search mode. " +
                "Supported values are: vector, keyword, hybrid.",
                nameof(mode))
        };
    }
}