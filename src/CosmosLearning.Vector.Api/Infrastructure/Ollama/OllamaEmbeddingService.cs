using CosmosLearning.Vector.Api.Options;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Vector.Api.Infrastructure.Ollama;

public sealed class OllamaEmbeddingService : IOllamaEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var embeddings =
            await GenerateEmbeddingsAsync(
                [text],
                cancellationToken);

        return embeddings[0];
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
            return [];

        var request = new OllamaEmbeddingRequest
        {
            Model = _options.EmbeddingModel,
            Input = texts
        };

        _logger.LogInformation(
            "Generating {Count} embeddings using Ollama model {Model}",
            texts.Count,
            _options.EmbeddingModel);

        using HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "api/embed",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaEmbeddingResponse>(
                    cancellationToken);

        if (result?.Embeddings is null ||
            result.Embeddings.Count != texts.Count)
        {
            throw new InvalidOperationException(
                $"Ollama returned {result?.Embeddings?.Count ?? 0} " +
                $"embeddings for {texts.Count} inputs.");
        }

        foreach (float[] embedding in result.Embeddings)
        {
            if (embedding.Length != _options.Dimensions)
            {
                throw new InvalidOperationException(
                    $"Expected {_options.Dimensions} dimensions " +
                    $"but Ollama returned {embedding.Length}.");
            }
        }

        _logger.LogInformation("Generated {Count} embeddings successfully.", result.Embeddings.Count);

        return result.Embeddings;
    }
}