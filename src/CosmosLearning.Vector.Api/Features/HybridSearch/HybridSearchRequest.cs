namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchRequest
{
    public string Query { get; set; } = string.Empty;

    public string? Category { get; set; }

    public decimal? MinimumPrice { get; set; }

    public decimal? MaximumPrice { get; set; }

    public int Top { get; set; } = 5;
}