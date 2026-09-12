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

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        VectorSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "Query is required.");
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

        return await _repository.SearchAsync(
            request.Query,
            top,
            cancellationToken);
    }
}