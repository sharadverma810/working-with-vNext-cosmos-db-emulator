namespace CosmosLearning.Vector.Api.Infrastructure.Ollama;

public interface IOllamaEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);
}