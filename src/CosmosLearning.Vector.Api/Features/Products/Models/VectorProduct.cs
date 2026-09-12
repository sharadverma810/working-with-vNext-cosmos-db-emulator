using Newtonsoft.Json;

namespace CosmosLearning.Vector.Api.Features.Products.Models;

public sealed class VectorProduct
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("searchText")]
    public string SearchText { get; set; } = string.Empty;

    [JsonProperty("embedding")]
    public float[] Embedding { get; set; } = [];
}