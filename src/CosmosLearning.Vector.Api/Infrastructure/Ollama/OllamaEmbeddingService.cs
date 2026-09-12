using System.Net.Http.Json;
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
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));
        }

        var request = new OllamaEmbeddingRequest
        {
            Model = _options.EmbeddingModel,
            Input = text
        };

        _logger.LogInformation("Generating embedding using Ollama model {Model}", _options.EmbeddingModel);

        using HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync("api/embed", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
                cancellationToken);

        if (result?.Embeddings is null ||
            result.Embeddings.Count == 0)
        {
            throw new InvalidOperationException(
                "Ollama returned no embedding.");
        }

        float[] embedding = result.Embeddings[0];

        if (embedding.Length != _options.Dimensions)
        {
            throw new InvalidOperationException(
                $"Expected {_options.Dimensions} dimensions " +
                $"but Ollama returned {embedding.Length}.");
        }

        _logger.LogInformation("Embedding generated successfully. Dimensions: {Dimensions}", embedding.Length);

        return embedding;
    }
}