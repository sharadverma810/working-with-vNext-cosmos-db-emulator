namespace CosmosLearning.Api.Features.ErrorHandling.Exceptions;

public sealed class CosmosThrottlingException : Exception
{
    public CosmosThrottlingException(
        string message,
        TimeSpan retryAfter,
        Exception? innerException = null)
        : base(message, innerException)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan RetryAfter { get; }
}