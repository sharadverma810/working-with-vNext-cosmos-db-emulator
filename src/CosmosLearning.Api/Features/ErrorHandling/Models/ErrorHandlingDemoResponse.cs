namespace CosmosLearning.Api.Features.ErrorHandling.Models;

public sealed record ErrorHandlingDemoResponse(
    string Message,
    string TraceId,
    DateTimeOffset Timestamp);