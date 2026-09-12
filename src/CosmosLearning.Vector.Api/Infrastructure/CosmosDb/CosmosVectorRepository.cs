using CosmosLearning.Vector.Api.Features.Products.Models;
using CosmosLearning.Vector.Api.Features.Products.Seed;
using CosmosLearning.Vector.Api.Features.VectorSearch;
using CosmosLearning.Vector.Api.Infrastructure.Ollama;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

public sealed class CosmosVectorRepository
{
    private readonly Container _container;
    private readonly IOllamaEmbeddingService _embeddingService;
    private readonly ILogger<CosmosVectorRepository> _logger;

    public CosmosVectorRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> options,
        IOllamaEmbeddingService embeddingService,
        ILogger<CosmosVectorRepository> logger)
    {
        _container = cosmosClient
            .GetDatabase(options.Value.DatabaseName)
            .GetContainer(options.Value.ContainerName);

        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<int> GetProductCountAsync(
        CancellationToken cancellationToken = default)
    {
        var queryDefinition =
            new QueryDefinition(
                "SELECT VALUE COUNT(1) FROM c");

        using FeedIterator<int> iterator =
            _container.GetItemQueryIterator<int>(
                queryDefinition);

        int count = 0;

        while (iterator.HasMoreResults)
        {
            FeedResponse<int> response =
                await iterator.ReadNextAsync(cancellationToken);

            count += response.FirstOrDefault();
        }

        return count;
    }

    public async Task<int> SeedProductsAsync(
        CancellationToken cancellationToken = default)
    {
        int existingCount =
            await GetProductCountAsync(cancellationToken);

        if (existingCount > 0)
        {
            _logger.LogInformation(
                "Products already exist. Count: {Count}",
                existingCount);

            return 0;
        }

        IReadOnlyList<VectorProduct> products =
            ProductSeedData.GetProducts();

        foreach (VectorProduct product in products)
        {
            product.SearchText =
                $"{product.Name}. " +
                $"{product.Category}. " +
                $"{product.Description}";

            product.Embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    product.SearchText,
                    cancellationToken);

            await _container.CreateItemAsync(
                product,
                new PartitionKey(product.Category),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Inserted product {ProductName}",
                product.Name);
        }

        return products.Count;
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string query,
        int top,
        CancellationToken cancellationToken = default)
    {
        float[] queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        var queryDefinition =
            new QueryDefinition(
                """
                SELECT TOP @top
                    c.id,
                    c.name,
                    c.category,
                    c.price,
                    c.description,
                    VectorDistance(c.embedding, @embedding) AS distance
                FROM c
                ORDER BY VectorDistance(c.embedding, @embedding)
                """)
            .WithParameter("@top", top)
            .WithParameter("@embedding", queryEmbedding);

        using FeedIterator<VectorSearchResult> iterator =
            _container.GetItemQueryIterator<VectorSearchResult>(
                queryDefinition);

        var results = new List<VectorSearchResult>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<VectorSearchResult> response =
                await iterator.ReadNextAsync(cancellationToken);

            results.AddRange(response);
        }

        return results;
    }
}