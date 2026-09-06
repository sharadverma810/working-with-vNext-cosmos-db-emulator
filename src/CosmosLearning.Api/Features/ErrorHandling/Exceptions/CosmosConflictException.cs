namespace CosmosLearning.Api.Features.ErrorHandling.Exceptions;

public sealed class CosmosConflictException : Exception
{
    public CosmosConflictException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}