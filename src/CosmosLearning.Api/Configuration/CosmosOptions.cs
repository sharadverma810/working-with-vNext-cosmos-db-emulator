namespace CosmosLearning.Api.Configuration;

public sealed class CosmosOptions
{
    public const string SectionName = "Cosmos";

    public required string Endpoint { get; init; }

    public required string AccountKey { get; init; }

    public required string DatabaseName { get; init; }

    public required string ContainerName { get; init; }
}