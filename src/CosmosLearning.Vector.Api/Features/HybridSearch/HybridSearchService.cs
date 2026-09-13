using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchService
{
    private readonly CosmosHybridRepository _repository;
    private readonly KeywordSearchService _keywordSearchService;

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

                        // RRF is not used in vector-only mode.
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

        /*
         * IMPORTANT:
         *
         * KeywordSearchService.Search() expects:
         *
         *     Search(query, documents)
         *
         * and returns KeywordSearchResult objects.
         */
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

                        // RRF is not used in keyword-only mode.
                        RrfScore = 0
                    })
            .ToList();
    }

    private async Task<IReadOnlyList<HybridSearchResult>> SearchHybridAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken)
    {
        /*
         * Retrieve more vector candidates than the requested Top value.
         *
         * Example:
         *
         *     Top = 5
         *     Vector candidates = 50
         *
         * This gives RRF a larger candidate pool to work with.
         */
        const int vectorCandidateCount = 50;

        var vectorCandidates =
            await _repository.VectorSearchAsync(
                request,
                vectorCandidateCount,
                cancellationToken);

        /*
         * Retrieve all documents matching the metadata filters.
         *
         * Keyword ranking is currently performed in the application
         * using our BM25-style implementation.
         */
        var keywordCandidates =
            await _repository.GetKeywordCandidatesAsync(
                request,
                cancellationToken);

        /*
         * IMPORTANT:
         *
         * KeywordSearchService.Search() expects:
         *
         *     Search(query, documents)
         */
        var keywordResults =
            _keywordSearchService.Search(
                request.Query,
                keywordCandidates);

        /*
         * Combine the vector ranking and keyword ranking
         * using Reciprocal Rank Fusion.
         */
        return ReciprocalRankFusion
            .Fuse(
                vectorCandidates,
                keywordResults,
                request.Top)
            .ToList();
    }

    private static HybridSearchMode ParseMode(string? mode)
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