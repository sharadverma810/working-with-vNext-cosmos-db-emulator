namespace CosmosLearning.Vector.Api.Features.VectorSearch;

public sealed class VectorSearchRequest
{
    public string Query { get; set; } = string.Empty;

    public int Top { get; set; } = 5;
}