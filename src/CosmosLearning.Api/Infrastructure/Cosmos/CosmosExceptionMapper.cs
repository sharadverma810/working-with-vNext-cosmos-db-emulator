using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Infrastructure.Cosmos;

public static class CosmosExceptionMapper
{
    public static int GetStatusCode(CosmosException exception)
    {
        return exception.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest =>
                StatusCodes.Status400BadRequest,

            System.Net.HttpStatusCode.NotFound =>
                StatusCodes.Status404NotFound,

            System.Net.HttpStatusCode.Conflict =>
                StatusCodes.Status409Conflict,

            System.Net.HttpStatusCode.TooManyRequests =>
                StatusCodes.Status429TooManyRequests,

            System.Net.HttpStatusCode.RequestTimeout =>
                StatusCodes.Status504GatewayTimeout,

            System.Net.HttpStatusCode.ServiceUnavailable =>
                StatusCodes.Status503ServiceUnavailable,

            _ =>
                StatusCodes.Status500InternalServerError
        };
    }

    public static bool IsTransient(CosmosException exception)
    {
        return exception.StatusCode is
            System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.InternalServerError
            or System.Net.HttpStatusCode.ServiceUnavailable;
    }
}