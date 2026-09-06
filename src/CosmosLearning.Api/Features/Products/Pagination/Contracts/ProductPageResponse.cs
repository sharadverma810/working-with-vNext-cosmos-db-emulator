using CosmosLearning.Api.Models;

namespace CosmosLearning.Api.Features.Products.Pagination.Contracts;

public sealed class ProductPageResponse
{
    public IReadOnlyList<ProductDocument> Items { get; init; }
        = Array.Empty<ProductDocument>();

    public int Count { get; init; }

    public int PageSize { get; init; }

    public string? ContinuationToken { get; init; }

    public bool HasMoreResults { get; init; }

    public double RequestCharge { get; init; }

    public string ActivityId { get; init; } = string.Empty;
}