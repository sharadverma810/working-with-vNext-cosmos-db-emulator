using CosmosLearning.Vector.Api.Features.HybridSearch;
using CosmosLearning.Vector.Api.Features.Products.Seed;
using CosmosLearning.Vector.Api.Infrastructure.Ollama;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Text;

namespace CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

public sealed class CosmosHybridRepository
{
    private readonly Container _container;
    private readonly IOllamaEmbeddingService _embeddingService;
    private readonly ILogger<CosmosHybridRepository> _logger;

    public CosmosHybridRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        IOllamaEmbeddingService embeddingService,
        ILogger<CosmosHybridRepository> logger)
    {
        var options = cosmosOptions.Value;

        _container =
            cosmosClient
                .GetDatabase(options.DatabaseName)
                .GetContainer(options.HybridContainerName);

        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<int> SeedAsync(
        int targetCount,
        CancellationToken cancellationToken = default)
    {
        var countQuery =
            new QueryDefinition(
                "SELECT VALUE COUNT(1) FROM c");

        using FeedIterator<int> iterator =
            _container.GetItemQueryIterator<int>(
                countQuery);

        int currentCount = 0;

        if (iterator.HasMoreResults)
        {
            FeedResponse<int> response =
                await iterator.ReadNextAsync(
                    cancellationToken);

            currentCount = response.FirstOrDefault();
        }

        if (currentCount >= targetCount)
        {
            _logger.LogInformation(
                "Hybrid catalog already contains {CurrentCount} products.",
                currentCount);

            return 0;
        }

        int productsToCreate =
            targetCount - currentCount;

        var products =
            LargeProductCatalog
                .Generate(productsToCreate)
                .ToList();

        for (int i = 0; i < products.Count; i++)
        {
            products[i].Id =
                $"hybrid-{currentCount + i + 1:0000}";
        }

        const int batchSize = 20;

        int inserted = 0;

        for (int offset = 0;
             offset < products.Count;
             offset += batchSize)
        {
            var batch =
                products
                    .Skip(offset)
                    .Take(batchSize)
                    .ToList();

            _logger.LogInformation(
                "Generating embeddings for hybrid batch {Start}-{End} of {Total}",
                offset + 1,
                Math.Min(
                    offset + batch.Count,
                    products.Count),
                products.Count);

            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(
                    batch
                        .Select(p => p.SearchText)
                        .ToList(),
                    cancellationToken);

            for (int i = 0; i < batch.Count; i++)
            {
                batch[i].Embedding =
                    embeddings[i];

                await _container.CreateItemAsync(
                    batch[i],
                    new PartitionKey(batch[i].Category),
                    cancellationToken:
                        cancellationToken);

                inserted++;
            }

            _logger.LogInformation(
                "Inserted {Inserted} / {Total} hybrid products",
                inserted,
                products.Count);
        }

        return inserted;
    }
    public async Task<IReadOnlyList<HybridSearchCandidate>> VectorSearchAsync(
    HybridSearchRequest request,
    int candidateCount,
    CancellationToken cancellationToken = default)
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
            c.searchText
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
            """
        ORDER BY VectorDistance(
            c.embedding,
            @embedding
        )
        """);

        var queryDefinition =
            new QueryDefinition(sql.ToString())
                .WithParameter("@top", candidateCount)
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

        using FeedIterator<HybridSearchCandidate> iterator =
            _container.GetItemQueryIterator<HybridSearchCandidate>(
                queryDefinition);

        var results =
            new List<HybridSearchCandidate>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<HybridSearchCandidate> response =
                await iterator.ReadNextAsync(
                    cancellationToken);

            results.AddRange(response);
        }

        return results;
    }
    public async Task<IReadOnlyList<HybridSearchCandidate>> GetKeywordCandidatesAsync(
    HybridSearchRequest request,
    CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder();

        sql.AppendLine(
            """
        SELECT
            c.id,
            c.name,
            c.category,
            c.price,
            c.description,
            c.searchText
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

        var queryDefinition =
            new QueryDefinition(sql.ToString());

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

        using FeedIterator<HybridSearchCandidate> iterator =
            _container.GetItemQueryIterator<HybridSearchCandidate>(
                queryDefinition);

        var results =
            new List<HybridSearchCandidate>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<HybridSearchCandidate> response =
                await iterator.ReadNextAsync(
                    cancellationToken);

            results.AddRange(response);
        }

        return results;
    }
    public async Task<IReadOnlyList<HybridSearchResult>> SearchAsync_Cloud_CosmosDb(
    HybridSearchRequest request,
    int top,
    CancellationToken cancellationToken = default)
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
            c.description
        FROM c
        WHERE 1 = 1
        """);

        if (!string.IsNullOrWhiteSpace(request.Category))
            sql.AppendLine(
                "AND c.category = @category");

        if (request.MinimumPrice.HasValue)
            sql.AppendLine(
                "AND c.price >= @minimumPrice");

        if (request.MaximumPrice.HasValue)
            sql.AppendLine(
                "AND c.price <= @maximumPrice");

        sql.AppendLine(
            """
        ORDER BY RANK RRF(
            VectorDistance(c.embedding, @embedding),
            FullTextScore(c.searchText, @searchText)
        )
        """);

        var queryDefinition =
            new QueryDefinition(sql.ToString())
                .WithParameter("@top", top)
                .WithParameter("@embedding", queryEmbedding)
                .WithParameter("@searchText", request.Query);

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

        using FeedIterator<HybridSearchResult> iterator =
            _container.GetItemQueryIterator<HybridSearchResult>(
                queryDefinition);

        var results =
            new List<HybridSearchResult>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<HybridSearchResult> response =
                await iterator.ReadNextAsync(
                    cancellationToken);

            results.AddRange(response);
        }

        return results;
    }
}