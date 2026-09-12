namespace CosmosLearning.Vector.Api.Options;

public sealed class CosmosOptions
{
    public const string SectionName = "Cosmos";

    public string Endpoint { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;
}

