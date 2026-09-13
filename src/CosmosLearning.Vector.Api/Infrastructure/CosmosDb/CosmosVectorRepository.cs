using CosmosLearning.Vector.Api.Features.Products.Models;
using CosmosLearning.Vector.Api.Features.Products.Seed;
using CosmosLearning.Vector.Api.Features.VectorSearch;
using CosmosLearning.Vector.Api.Infrastructure.Ollama;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Text;

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

    public async Task<int> GetProductCountAsync(CancellationToken cancellationToken = default)
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

    public async Task<int> SeedProductsAsync(CancellationToken cancellationToken = default)
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
    public async Task<int> SeedLargeCatalogAsync(int targetCount, CancellationToken cancellationToken = default)
    {
        var countQuery =
            new QueryDefinition("SELECT VALUE COUNT(1) FROM c");

        using FeedIterator<int> iterator = _container.GetItemQueryIterator<int>(countQuery);

        int currentCount = 0;

        if (iterator.HasMoreResults)
        {
            FeedResponse<int> response = await iterator.ReadNextAsync(cancellationToken);

            currentCount = response.FirstOrDefault();
        }

        if (currentCount >= targetCount)
        {
            _logger.LogInformation(
                "Catalog already contains {CurrentCount} products. Target is {TargetCount}.",
                currentCount,
                targetCount);

            return 0;
        }

        int productsToCreate = targetCount - currentCount;

        var products = LargeProductCatalog.Generate(productsToCreate).ToList();

        // Make IDs unique even when the existing 12 seed products are present.
        for (int i = 0; i < products.Count; i++)
        {
            products[i].Id = $"catalog-{currentCount + i + 1:0000}";
        }

        const int batchSize = 20;

        int inserted = 0;

        for (int offset = 0; offset < products.Count; offset += batchSize)
        {
            var batch =
                products
                    .Skip(offset)
                    .Take(batchSize)
                    .ToList();

            _logger.LogInformation(
                "Embedding catalog batch {Current} - {End} of {Total}",
                offset + 1,
                Math.Min(offset + batch.Count, products.Count),
                products.Count);

            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(
                    batch.Select(x => x.SearchText).ToList(),
                    cancellationToken);

            for (int i = 0; i < batch.Count; i++)
            {
                batch[i].Embedding = embeddings[i];

                await _container.CreateItemAsync(
                    batch[i],
                    new PartitionKey(batch[i].Category),
                    cancellationToken: cancellationToken);

                inserted++;
            }

            _logger.LogInformation("Inserted {Inserted} / {Total} catalog products", inserted, products.Count);
        }

        return inserted;
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(VectorSearchRequest request, int top, CancellationToken cancellationToken = default)
    {
        float[] queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                request.Query,
                cancellationToken);

        var sql = new StringBuilder();

        sql.AppendLine(
            """
        SELECT TOP @top
            c.id,
            c.name,
            c.category,
            c.price,
            c.description,
            VectorDistance(c.embedding, @embedding) AS distance
        FROM c
        WHERE 1 = 1
        """);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            sql.AppendLine(
                "AND c.category = @category");
        }

        if (request.MinimumPrice.HasValue)
        {
            sql.AppendLine(
                "AND c.price >= @minimumPrice");
        }

        if (request.MaximumPrice.HasValue)
        {
            sql.AppendLine(
                "AND c.price <= @maximumPrice");
        }

        sql.AppendLine(
            "ORDER BY VectorDistance(c.embedding, @embedding)");

        var queryDefinition =
            new QueryDefinition(sql.ToString())
                .WithParameter("@top", top)
                .WithParameter("@embedding", queryEmbedding);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            queryDefinition =
                queryDefinition.WithParameter(
                    "@category",
                    request.Category);
        }

        if (request.MinimumPrice.HasValue)
        {
            queryDefinition =
                queryDefinition.WithParameter(
                    "@minimumPrice",
                    request.MinimumPrice.Value);
        }

        if (request.MaximumPrice.HasValue)
        {
            queryDefinition =
                queryDefinition.WithParameter(
                    "@maximumPrice",
                    request.MaximumPrice.Value);
        }

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