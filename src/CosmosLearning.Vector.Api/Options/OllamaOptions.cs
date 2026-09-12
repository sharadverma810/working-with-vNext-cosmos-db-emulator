namespace CosmosLearning.Vector.Api.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = string.Empty;

    public string EmbeddingModel { get; set; } = string.Empty;

    public int Dimensions { get; set; }
}

