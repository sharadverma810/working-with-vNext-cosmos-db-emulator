namespace CosmosLearning.Bootstrap;

public sealed class BootstrapOptions
{
    public required string Endpoint { get; init; }

    public required string AccountKey { get; init; }

    public required string DatabaseName { get; init; }

    public required string ContainerName { get; init; }

    public int DocumentCount { get; init; } = 5_000;

    public int ProgressInterval { get; init; } = 500;
}