using System.Diagnostics.Metrics;

namespace CosmosLearning.Api.Features.Products.Pagination.Telemetry;

public static class PaginationMetrics
{
    public const string MeterName =
        "CosmosLearning.Api.ProductPagination";

    private static readonly Meter Meter =
        new(MeterName);


    public static readonly Counter<long> Requests =
        Meter.CreateCounter<long>(
            "product_pagination_requests_total",
            unit: "requests",
            description:
                "Total number of product pagination requests.");


    public static readonly Counter<long> SuccessfulRequests =
        Meter.CreateCounter<long>(
            "product_pagination_success_total",
            unit: "requests",
            description:
                "Total number of successful product pagination requests.");


    public static readonly Counter<long> ValidationFailures =
        Meter.CreateCounter<long>(
            "product_pagination_validation_failures_total",
            unit: "requests",
            description:
                "Total number of invalid product pagination requests.");


    public static readonly Counter<long> Errors =
        Meter.CreateCounter<long>(
            "product_pagination_errors_total",
            unit: "errors",
            description:
                "Total number of product pagination errors.");


    public static readonly Histogram<double> Duration =
        Meter.CreateHistogram<double>(
            "product_pagination_duration",
            unit: "ms",
            description:
                "Duration of product pagination operations.");


    public static readonly Histogram<long> ItemsReturned =
        Meter.CreateHistogram<long>(
            "product_pagination_items_returned",
            unit: "items",
            description:
                "Number of products returned by pagination.");


    public static readonly Histogram<double> RequestCharge =
        Meter.CreateHistogram<double>(
            "product_pagination_request_charge",
            unit: "RU",
            description:
                "Cosmos DB Request Units consumed by pagination.");
}