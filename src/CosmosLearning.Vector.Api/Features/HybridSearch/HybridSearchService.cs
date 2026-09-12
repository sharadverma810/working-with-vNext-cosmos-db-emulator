using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchService
{
    private readonly CosmosHybridRepository _repository;

    public HybridSearchService(
        CosmosHybridRepository repository)
    {
        _repository = repository;
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
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "Query is required.");
        }

        int top = request.Top;

        if (top < 1)
            top = 5;

        if (top > 20)
            top = 20;

        if (request.MinimumPrice.HasValue &&
            request.MaximumPrice.HasValue &&
            request.MinimumPrice >
            request.MaximumPrice)
        {
            throw new ArgumentException(
                "MinimumPrice cannot be greater than MaximumPrice.");
        }

        return await _repository.SearchAsync(
            request,
            top,
            cancellationToken);
    }
}