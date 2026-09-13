namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchRequest
{
    public string Query { get; set; } = string.Empty;

    public string Mode { get; set; } = "hybrid";

    public string? Category { get; set; }

    public decimal? MinimumPrice { get; set; }

    public decimal? MaximumPrice { get; set; }

    public int Top { get; set; } = 5;

    public double VectorWeight { get; set; } = 1.0;

    public double KeywordWeight { get; set; } = 1.0;
}