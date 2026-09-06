using System.Diagnostics;

namespace CosmosLearning.Api.Features.Products.Pagination.Telemetry;

public static class PaginationActivitySource
{
    public const string SourceName =
        "CosmosLearning.Api.ProductPagination";

    public static readonly ActivitySource Source =
        new(SourceName);
}