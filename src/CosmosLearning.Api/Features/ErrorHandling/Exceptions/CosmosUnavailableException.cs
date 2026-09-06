namespace CosmosLearning.Api.Features.ErrorHandling.Exceptions;

public sealed class CosmosUnavailableException : Exception
{
    public CosmosUnavailableException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}