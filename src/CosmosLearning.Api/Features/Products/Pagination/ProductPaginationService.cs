using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.Products.Pagination.Contracts;
using CosmosLearning.Api.Features.Products.Pagination.Telemetry;
using CosmosLearning.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace CosmosLearning.Api.Features.Products.Pagination;

public sealed class ProductPaginationService
{
    private readonly Container _container;

    private readonly PaginationTelemetry _telemetry;

    private readonly ILogger<ProductPaginationService>
        _logger;


    public ProductPaginationService(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        PaginationTelemetry telemetry,
        ILogger<ProductPaginationService> logger)
    {
        ArgumentNullException.ThrowIfNull(
            cosmosClient);

        ArgumentNullException.ThrowIfNull(
            cosmosOptions);

        ArgumentNullException.ThrowIfNull(
            telemetry);

        ArgumentNullException.ThrowIfNull(
            logger);


        _container =
            cosmosClient.GetContainer(
                cosmosOptions.Value.DatabaseName,
                cosmosOptions.Value.ContainerName);


        _telemetry = telemetry;

        _logger = logger;
    }


    public async Task<ProductPageResponse> GetPageAsync(
        ProductPageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);


        using var serviceActivity =
            _telemetry.StartServiceActivity(
                request);


        var serviceStopwatch =
            Stopwatch.StartNew();


        try
        {
            // --------------------------------------
            // Build Query
            // --------------------------------------

            var query =
                ProductPaginationQueryBuilder.Build(
                    request);


            // --------------------------------------
            // Cosmos Request Options
            // --------------------------------------

            var requestOptions =
                new QueryRequestOptions
                {
                    MaxItemCount =
                        request.PageSize
                };


            // --------------------------------------
            // Create Iterator
            // --------------------------------------

            using FeedIterator<ProductDocument>
                iterator =
                    _container
                        .GetItemQueryIterator<ProductDocument>(
                            query,
                            request.ContinuationToken,
                            requestOptions);


            // --------------------------------------
            // No More Results
            // --------------------------------------

            if (!iterator.HasMoreResults)
            {
                serviceStopwatch.Stop();


                var emptyResult =
                    new ProductPageResponse
                    {
                        Items =
                            Array.Empty<ProductDocument>(),

                        Count = 0,

                        PageSize =
                            request.PageSize,

                        ContinuationToken =
                            null,

                        HasMoreResults =
                            false,

                        RequestCharge = 0,

                        ActivityId =
                            string.Empty
                    };


                _telemetry.RecordSuccess(
                    serviceActivity,
                    itemCount: 0,
                    requestCharge: 0,
                    durationMilliseconds:
                        serviceStopwatch
                            .Elapsed
                            .TotalMilliseconds);


                return emptyResult;
            }


            // --------------------------------------
            // Cosmos Read
            // --------------------------------------

            // --------------------------------------
            // Cosmos Read Telemetry
            // --------------------------------------

            using var cosmosActivity =
                _telemetry.StartCosmosReadActivity();

            var cosmosStopwatch =
                Stopwatch.StartNew();

            FeedResponse<ProductDocument> response;

            try
            {
                response =
                    await iterator.ReadNextAsync(
                        cancellationToken);

                cosmosStopwatch.Stop();

                cosmosActivity?.SetTag(
                    "db.system",
                    "azure.cosmosdb");

                cosmosActivity?.SetTag(
                    "db.operation",
                    "query");

                cosmosActivity?.SetTag(
                    "cosmos.request_charge",
                    response.RequestCharge);

                cosmosActivity?.SetTag(
                    "cosmos.activity_id",
                    response.ActivityId);

                cosmosActivity?.SetTag(
                    "cosmos.has_more_results",
                    !string.IsNullOrWhiteSpace(
                        response.ContinuationToken));

                cosmosActivity?.SetTag(
                    "cosmos.duration_ms",
                    cosmosStopwatch.Elapsed.TotalMilliseconds);

                cosmosActivity?.SetStatus(
                    ActivityStatusCode.Ok);
            }
            catch (Exception exception)
            {
                cosmosStopwatch.Stop();

                cosmosActivity?.RecordException(
                    exception);

                cosmosActivity?.SetStatus(
                    ActivityStatusCode.Error,
                    exception.Message);

                cosmosActivity?.SetTag(
                    "cosmos.duration_ms",
                    cosmosStopwatch.Elapsed.TotalMilliseconds);

                throw;
            }


            // --------------------------------------
            // Cosmos Activity Tags
            // --------------------------------------

            cosmosActivity?.SetTag(
                "db.system",
                "azure.cosmosdb");


            cosmosActivity?.SetTag(
                "db.operation",
                "query");


            cosmosActivity?.SetTag(
                "cosmos.request_charge",
                response.RequestCharge);


            cosmosActivity?.SetTag(
                "cosmos.activity_id",
                response.ActivityId);


            cosmosActivity?.SetTag(
                "cosmos.has_more_results",
                !string.IsNullOrWhiteSpace(
                    response.ContinuationToken));


            cosmosActivity?.SetTag(
                "cosmos.duration_ms",
                cosmosStopwatch
                    .Elapsed
                    .TotalMilliseconds);


            cosmosActivity?.SetStatus(
                ActivityStatusCode.Ok);


            // --------------------------------------
            // Convert Response
            // --------------------------------------

            var items =
                response.ToList();


            // --------------------------------------
            // Application Logging
            // --------------------------------------

            _logger.LogInformation(
                """
                Product page retrieved.
                Count: {Count}.
                PageSize: {PageSize}.
                HasContinuationToken: {HasContinuationToken}.
                ActivityId: {ActivityId}.
                RequestCharge: {RequestCharge}.
                """,
                items.Count,
                request.PageSize,
                !string.IsNullOrWhiteSpace(
                    response.ContinuationToken),
                response.ActivityId,
                response.RequestCharge);


            // --------------------------------------
            // Response
            // --------------------------------------

            var result =
                new ProductPageResponse
                {
                    Items =
                        items,

                    Count =
                        items.Count,

                    PageSize =
                        request.PageSize,

                    ContinuationToken =
                        response.ContinuationToken,

                    HasMoreResults =
                        !string.IsNullOrWhiteSpace(
                            response.ContinuationToken),

                    RequestCharge =
                        response.RequestCharge,

                    ActivityId =
                        response.ActivityId
                };


            // --------------------------------------
            // Service Telemetry
            // --------------------------------------

            serviceStopwatch.Stop();


            _telemetry.RecordSuccess(
                serviceActivity,
                itemCount:
                    result.Count,

                requestCharge:
                    result.RequestCharge,

                durationMilliseconds:
                    serviceStopwatch
                        .Elapsed
                        .TotalMilliseconds);


            return result;
        }
        catch (Exception exception)
        {
            serviceStopwatch.Stop();


            _telemetry.RecordFailure(
                serviceActivity,
                exception,
                serviceStopwatch
                    .Elapsed
                    .TotalMilliseconds);


            throw;
        }
    }
}