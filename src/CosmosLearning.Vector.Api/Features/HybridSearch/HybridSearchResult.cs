using System.Text.Json.Serialization;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class HybridSearchResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("vectorRank")]
    public int? VectorRank { get; set; }

    [JsonPropertyName("keywordRank")]
    public int? KeywordRank { get; set; }

    [JsonPropertyName("rrfScore")]
    public double RrfScore { get; set; }
}