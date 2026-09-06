namespace CosmosLearning.Api.Features.Products.Pagination;

public sealed class ProductPageRequest
{
    public int PageSize { get; init; } = 10;

    public string? Category { get; init; }

    public bool? IsActive { get; init; }

    public decimal? MinimumPrice { get; init; }

    public decimal? MaximumPrice { get; init; }

    // This value comes from the HTTP header.
    // It is passed to Cosmos DB without modification.
    public string? ContinuationToken { get; init; }
}