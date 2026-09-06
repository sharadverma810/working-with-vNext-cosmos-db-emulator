namespace CosmosLearning.Api.Configuration;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public int RetryCount { get; init; } = 3;

    public int RetryBaseDelayMilliseconds { get; init; } = 500;

    public int TimeoutSeconds { get; init; } = 30;

    public int CircuitBreakerFailureRatio { get; init; } = 50;

    public int CircuitBreakerMinimumThroughput { get; init; } = 5;

    public int CircuitBreakerBreakDurationSeconds { get; init; } = 30;
}