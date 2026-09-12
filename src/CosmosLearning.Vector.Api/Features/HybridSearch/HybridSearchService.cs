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

    public async Task<int> SeedAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count < 1000)
        {
            throw new ArgumentException(
                "Hybrid catalog must contain at least 1000 products.");
        }

        if (count > 5000)
        {
            throw new ArgumentException(
                "For this local demo, maximum catalog size is 5000 products.");
        }

        return await _repository.SeedAsync(
            count,
            cancellationToken);
    }

    public async Task<IReadOnlyList<HybridSearchResult>> SearchAsync(
        HybridSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        const int candidateCount = 50;

        // --------------------------------------------
        // 1. Semantic retrieval
        // --------------------------------------------

        IReadOnlyList<HybridSearchCandidate> vectorCandidates =
            await _repository.VectorSearchAsync(
                request,
                candidateCount,
                cancellationToken);

        // --------------------------------------------
        // 2. Lexical retrieval
        // --------------------------------------------

        IReadOnlyList<HybridSearchCandidate> keywordCandidates =
            await _repository.GetKeywordCandidatesAsync(
                request,
                cancellationToken);

        IReadOnlyList<KeywordSearchResult> keywordResults =
            _keywordSearchService.Search(
                request.Query,
                keywordCandidates);

        // --------------------------------------------
        // 3. Reciprocal Rank Fusion
        // --------------------------------------------

        return ReciprocalRankFusion.Fuse(
            vectorCandidates,
            keywordResults,
            request.Top);
    }

    private static void ValidateRequest(
        HybridSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "Query is required.");
        }

        if (request.MinimumPrice.HasValue &&
            request.MaximumPrice.HasValue &&
            request.MinimumPrice >
            request.MaximumPrice)
        {
            throw new ArgumentException(
                "MinimumPrice cannot be greater than MaximumPrice.");
        }
    }
}