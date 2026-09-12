namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchCandidate
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;

    public string SearchText { get; set; } = string.Empty;
}