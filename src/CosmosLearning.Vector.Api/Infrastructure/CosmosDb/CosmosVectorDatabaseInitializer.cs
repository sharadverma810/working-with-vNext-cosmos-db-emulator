using System.Collections.ObjectModel;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

public sealed class CosmosVectorDatabaseInitializer
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosOptions _options;
    private readonly OllamaOptions _ollamaOptions;
    private readonly ILogger<CosmosVectorDatabaseInitializer> _logger;

    public CosmosVectorDatabaseInitializer(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        IOptions<OllamaOptions> ollamaOptions,
        ILogger<CosmosVectorDatabaseInitializer> logger)
    {
        _cosmosClient = cosmosClient;
        _options = cosmosOptions.Value;
        _ollamaOptions = ollamaOptions.Value;
        _logger = logger;
    }

    public async Task<Container> InitializeAsync(
       CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Initializing Cosmos DB database {DatabaseName}",
            _options.DatabaseName);

        DatabaseResponse databaseResponse =
            await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                _options.DatabaseName,
                cancellationToken: cancellationToken);

        Database database = databaseResponse.Database;

        var embeddings = new Collection<Embedding>
    {
        new Embedding
        {
            Path = "/embedding",
            DataType = VectorDataType.Float32,
            DistanceFunction = DistanceFunction.Cosine,
            Dimensions = _ollamaOptions.Dimensions
        }
    };

        var containerProperties =
            new ContainerProperties(
                id: _options.ContainerName,
                partitionKeyPath: "/category")
            {
                VectorEmbeddingPolicy =
                    new VectorEmbeddingPolicy(embeddings),

                IndexingPolicy = new IndexingPolicy
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent,

                    VectorIndexes =
                    [
                        new VectorIndexPath
                    {
                        Path = "/embedding",
                        Type = VectorIndexType.DiskANN
                    }
                    ]
                }
            };

        // The vector field is handled by the vector index.
        // Keep it out of the normal property index.
        containerProperties.IndexingPolicy
            .ExcludedPaths
            .Add(new ExcludedPath
            {
                Path = "/embedding/*"
            });

        _logger.LogInformation(
            "Creating or getting vector container {ContainerName}",
            _options.ContainerName);

        ContainerResponse containerResponse =
            await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Vector database initialization completed.");

        return containerResponse.Container;
    }
}