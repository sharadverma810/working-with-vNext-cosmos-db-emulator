using System.Collections.ObjectModel;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Vector.Api.Infrastructure.CosmosDb;

public sealed class CosmosHybridDatabaseInitializer
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosOptions _options;
    private readonly OllamaOptions _ollamaOptions;
    private readonly ILogger<CosmosHybridDatabaseInitializer> _logger;

    private const string HybridContainerName = "HybridProducts";

    public CosmosHybridDatabaseInitializer(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        IOptions<OllamaOptions> ollamaOptions,
        ILogger<CosmosHybridDatabaseInitializer> logger)
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
            "Initializing hybrid search database {DatabaseName}",
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

        var fullTextPaths =
            new Collection<FullTextPath>
            {
                new FullTextPath
                {
                    Path = "/searchText",
                    Language = "en-US"
                }
            };

        var fullTextPolicy = new FullTextPolicy
        {
            DefaultLanguage = "en-US",
            FullTextPaths = fullTextPaths
        };

        var containerProperties =
            new ContainerProperties(
                HybridContainerName,
                partitionKeyPath: "/category")
            {
                VectorEmbeddingPolicy =
                    new VectorEmbeddingPolicy(embeddings),

                FullTextPolicy = fullTextPolicy,

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
                    ],

                    FullTextIndexes =
                    [
                        new FullTextIndexPath
                        {
                            Path = "/searchText"
                        }
                    ]
                }
            };

        containerProperties.IndexingPolicy
            .ExcludedPaths
            .Add(
                new ExcludedPath
                {
                    Path = "/embedding/*"
                });

        _logger.LogInformation(
            "Creating or getting hybrid search container {ContainerName}",
            HybridContainerName);

        ContainerResponse response =
            await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Hybrid search container initialization completed.");

        return response.Container;
    }
}