using CosmosLearning.Api.Features.Products.Pagination.Contracts;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace CosmosLearning.Api.Features.Products.Pagination.Telemetry;

public sealed class PaginationTelemetry
{
    public Activity? StartControllerActivity(
        ProductPageRequest request)
    {
        var activity =
            PaginationActivitySource.Source.StartActivity(
                "ProductPagination.GetPage",
                ActivityKind.Internal);

        AddRequestTags(
            activity,
            request);

        return activity;
    }


    public Activity? StartServiceActivity(
        ProductPageRequest request)
    {
        var activity =
            PaginationActivitySource.Source.StartActivity(
                "ProductPagination.Service.GetPage",
                ActivityKind.Internal);

        AddRequestTags(
            activity,
            request);

        return activity;
    }


    public Activity? StartQueryBuilderActivity(
        ProductPageRequest request)
    {
        var activity =
            PaginationActivitySource.Source.StartActivity(
                "ProductPagination.QueryBuilder.Build",
                ActivityKind.Internal);

        AddRequestTags(
            activity,
            request);

        return activity;
    }


    public Activity? StartCosmosReadActivity()
    {
        return PaginationActivitySource.Source.StartActivity(
            "ProductPagination.Cosmos.ReadNext",
            ActivityKind.Client);
    }


    public void RecordRequest(
        ProductPageRequest request)
    {
        PaginationMetrics.Requests.Add(
            1,
            new KeyValuePair<string, object?>(
                "page_size",
                request.PageSize));
    }


    public void RecordValidationFailure()
    {
        PaginationMetrics.ValidationFailures.Add(1);
    }


    public void RecordSuccess(
        Activity? activity,
        int itemCount,
        double requestCharge,
        double durationMilliseconds)
    {
        activity?.SetTag(
            "pagination.items_returned",
            itemCount);

        activity?.SetTag(
            "cosmos.request_charge",
            requestCharge);

        activity?.SetTag(
            "pagination.duration_ms",
            durationMilliseconds);

        activity?.SetStatus(
            ActivityStatusCode.Ok);


        PaginationMetrics.SuccessfulRequests.Add(1);


        PaginationMetrics.ItemsReturned.Record(
            itemCount);


        PaginationMetrics.RequestCharge.Record(
            requestCharge);


        PaginationMetrics.Duration.Record(
            durationMilliseconds);
    }


    public void RecordFailure(
        Activity? activity,
        Exception exception,
        double durationMilliseconds)
    {
        activity?.SetStatus(
            ActivityStatusCode.Error,
            exception.Message);

        activity?.RecordException(
            exception);

        activity?.SetTag(
            "pagination.duration_ms",
            durationMilliseconds);


        PaginationMetrics.Errors.Add(
            1,
            new KeyValuePair<string, object?>(
                "exception.type",
                exception.GetType().Name));


        PaginationMetrics.Duration.Record(
            durationMilliseconds);
    }


    private static void AddRequestTags(
        Activity? activity,
        ProductPageRequest request)
    {
        if (activity is null)
        {
            return;
        }


        activity.SetTag(
            "pagination.page_size",
            request.PageSize);


        activity.SetTag(
            "pagination.has_continuation_token",
            !string.IsNullOrWhiteSpace(
                request.ContinuationToken));


        activity.SetTag(
            "pagination.has_category_filter",
            !string.IsNullOrWhiteSpace(
                request.Category));


        activity.SetTag(
            "pagination.has_is_active_filter",
            request.IsActive.HasValue);


        activity.SetTag(
            "pagination.has_minimum_price_filter",
            request.MinimumPrice.HasValue);


        activity.SetTag(
            "pagination.has_maximum_price_filter",
            request.MaximumPrice.HasValue);
    }
}