namespace CosmosLearning.Api.Features.ErrorHandling.Exceptions;

public sealed class CosmosResourceNotFoundException : Exception
{
    public CosmosResourceNotFoundException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}