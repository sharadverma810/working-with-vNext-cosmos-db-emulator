using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

namespace CosmosLearning.Vector.Api.Features.VectorSearch;

public sealed class VectorSearchService
{
    private readonly CosmosVectorRepository _repository;

    public VectorSearchService(
        CosmosVectorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(VectorSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException("Query is required.");
        }

        int top = request.Top;

        if (top < 1)
        {
            top = 5;
        }

        if (top > 20)
        {
            top = 20;
        }

        if (request.MinimumPrice.HasValue &&
            request.MaximumPrice.HasValue &&
            request.MinimumPrice > request.MaximumPrice)
        {
            throw new ArgumentException("MinimumPrice cannot be greater than MaximumPrice.");
        }

        return await _repository.SearchAsync(request, top, cancellationToken);
    }

    public async Task<int> SeedLargeCatalogAsync(int targetCount, CancellationToken cancellationToken = default)
    {
        if (targetCount < 1000)
            throw new ArgumentException("The large catalog target must be at least 1000 products.");

        if (targetCount > 5000)
            throw new ArgumentException("For this local demo, the maximum catalog size is 5000 products.");

        return await _repository.SeedLargeCatalogAsync(targetCount, cancellationToken);
    }
}