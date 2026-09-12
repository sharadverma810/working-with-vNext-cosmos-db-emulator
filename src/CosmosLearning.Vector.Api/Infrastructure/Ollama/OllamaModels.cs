using System.Text.Json.Serialization;

namespace CosmosLearning.Vector.Api.Infrastructure.Ollama;

public sealed class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public IReadOnlyList<string> Input { get; set; } = [];
}

public sealed class OllamaEmbeddingResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("embeddings")]
    public List<float[]> Embeddings { get; set; } = [];
}