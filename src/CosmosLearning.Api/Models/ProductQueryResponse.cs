namespace CosmosLearning.Api.Models;

public sealed record ProductQueryResponse(
    IReadOnlyList<ProductDocument> Items,
    int Count,
    string? ContinuationToken,
    double? RequestCharge,
    string? ActivityId);