using CosmosLearning.Api.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Bootstrap;

public sealed class BootstrapRunner(BootstrapOptions options, ProductDataGenerator generator)
{
    public async Task RunAsync(bool reset, CancellationToken cancellationToken)
    {
        var products = generator.Generate(options.DocumentCount);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        Console.WriteLine($"Generating {products.Count:N0} sample documents...");

        using var cosmosClient = new CosmosClient(
            options.Endpoint,
            options.AccountKey,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });

        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            options.DatabaseName,
            cancellationToken: cancellationToken);
        var database = databaseResponse.Database;
        Console.WriteLine($"Database ready: {database.Id}");

        if (reset)
        {
            Console.WriteLine($"Resetting container: {options.ContainerName}");

            try
            {
                await database.GetContainer(options.ContainerName).DeleteContainerAsync(
                    cancellationToken: cancellationToken);
                Console.WriteLine("Existing sample container deleted.");
            }
            catch (CosmosException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("Sample container did not exist; continuing with recreation.");
            }
        }

        var containerResponse = await database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(options.ContainerName, "/category"),
            cancellationToken: cancellationToken);
        var container = containerResponse.Container;
        Console.WriteLine($"Container ready: {container.Id} (partition key: /category)");

        var inserted = 0;
        var failures = new List<string>();

        foreach (var product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await container.CreateItemAsync(
                    product,
                    new PartitionKey(product.Category),
                    cancellationToken: cancellationToken);

                inserted++;

                if (inserted % options.ProgressInterval == 0 || inserted == products.Count)
                {
                    Console.WriteLine($"Inserted {inserted:N0} / {products.Count:N0}");
                }
            }
            catch (CosmosException exception)
            {
                failures.Add($"{product.Id}: HTTP {(int)exception.StatusCode} {exception.StatusCode}");
                Console.Error.WriteLine($"Insert failed for {product.Id}: {(int)exception.StatusCode} {exception.StatusCode}");
            }
        }

        stopwatch.Stop();
        Console.WriteLine();
        Console.WriteLine(reset ? "Reset completed." : "Bootstrap completed.");
        Console.WriteLine($"Database: {options.DatabaseName}");
        Console.WriteLine($"Container: {options.ContainerName}");
        Console.WriteLine($"Documents generated: {products.Count:N0}");
        Console.WriteLine($"Documents inserted: {inserted:N0}");
        Console.WriteLine($"Failed inserts: {failures.Count:N0}");
        Console.WriteLine($"Duration: {stopwatch.Elapsed}");

        if (failures.Count > 0)
        {
            throw new InvalidOperationException($"The operation completed with {failures.Count} failed inserts.");
        }
    }
}