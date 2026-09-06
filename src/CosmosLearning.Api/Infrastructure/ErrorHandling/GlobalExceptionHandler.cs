using System.Diagnostics;
using CosmosLearning.Api.Features.ErrorHandling.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Api.Infrastructure.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
     HttpContext httpContext,
     Exception exception,
     CancellationToken cancellationToken)
    {
        var traceId =
            Activity.Current?.Id
            ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            traceId);

        var problemDetails =
            CreateProblemDetails(
                httpContext,
                exception,
                traceId);

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception,
        string traceId)
    {
        var problemDetails = exception switch
        {
            CosmosResourceNotFoundException =>
                CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    exception.Message),

            CosmosConflictException =>
                CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    exception.Message),

            CosmosThrottlingException throttlingException =>
                CreateThrottlingProblem(
                    httpContext,
                    throttlingException),

            CosmosUnavailableException =>
                CreateProblem(
                    StatusCodes.Status503ServiceUnavailable,
                    "Service unavailable",
                    exception.Message),

            OperationCanceledException =>
                CreateProblem(
                    StatusCodes.Status408RequestTimeout,
                    "Request cancelled",
                    "The operation was cancelled before it completed."),

            _ =>
                CreateProblem(
                    StatusCodes.Status500InternalServerError,
                    "Unexpected error",
                    "An unexpected error occurred while processing the request.")
        };

        problemDetails.Instance =
            httpContext.Request.Path;

        problemDetails.Extensions["traceId"] =
            traceId;

        problemDetails.Extensions["timestamp"] =
            DateTimeOffset.UtcNow;

        return problemDetails;
    }

    private static ProblemDetails CreateThrottlingProblem(
        HttpContext httpContext,
        CosmosThrottlingException exception)
    {
        var retryAfterSeconds =
            Math.Max(
                1,
                (int)Math.Ceiling(
                    exception.RetryAfter.TotalSeconds));

        httpContext.Response.Headers.RetryAfter =
            retryAfterSeconds.ToString();

        return CreateProblem(
            StatusCodes.Status429TooManyRequests,
            "Too many requests",
            exception.Message);
    }

    private static ProblemDetails CreateProblem(
        int status,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
}