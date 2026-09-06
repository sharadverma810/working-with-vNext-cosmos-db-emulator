using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.Products.Pagination.Contracts;
using CosmosLearning.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace CosmosLearning.Api.Features.Products.Pagination;

public sealed class ProductPaginationService
{
    private readonly Container _container;
    private readonly ILogger<ProductPaginationService> _logger;

    public ProductPaginationService(
        CosmosClient cosmosClient,
        IOptions<CosmosOptions> cosmosOptions,
        ILogger<ProductPaginationService> logger)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient);
        ArgumentNullException.ThrowIfNull(cosmosOptions);
        ArgumentNullException.ThrowIfNull(logger);

        _container = cosmosClient.GetContainer(
            cosmosOptions.Value.DatabaseName,
            cosmosOptions.Value.ContainerName);

        _logger = logger;
    }

    public async Task<ProductPageResponse> GetPageAsync(
        ProductPageRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = ProductPaginationQueryBuilder.Build(request);

        var requestOptions = new QueryRequestOptions
        {
            MaxItemCount = request.PageSize
        };

        using FeedIterator<ProductDocument> iterator =
            _container.GetItemQueryIterator<ProductDocument>(
                query,
                request.ContinuationToken,
                requestOptions);

        if (!iterator.HasMoreResults)
        {
            return new ProductPageResponse
            {
                Items = Array.Empty<ProductDocument>(),
                Count = 0,
                PageSize = request.PageSize,
                ContinuationToken = null,
                HasMoreResults = false,
                RequestCharge = 0,
                ActivityId = string.Empty
            };
        }

        FeedResponse<ProductDocument> response =
            await iterator.ReadNextAsync(cancellationToken);

        var items = response.ToList();

        _logger.LogInformation(
            """
            Product page retrieved.
            Count: {Count}.
            PageSize: {PageSize}.
            HasContinuationToken: {HasContinuationToken}.
            ActivityId: {ActivityId}.
            """,
            items.Count,
            request.PageSize,
            !string.IsNullOrWhiteSpace(response.ContinuationToken),
            response.ActivityId);

        return new ProductPageResponse
        {
            Items = items,

            Count = items.Count,

            PageSize = request.PageSize,

            ContinuationToken = response.ContinuationToken,

            HasMoreResults =
                !string.IsNullOrWhiteSpace(
                    response.ContinuationToken),

            RequestCharge = response.RequestCharge,

            ActivityId = response.ActivityId
        };
    }

    private static QueryDefinition BuildQuery(
        ProductPageRequest request)
    {
        var conditions = new List<string>();

        var query = new QueryDefinition(
            """
            SELECT *
            FROM c
            """);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            conditions.Add(
                "c.category = @category");

            query.WithParameter(
                "@category",
                request.Category);
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add(
                "c.isActive = @isActive");

            query.WithParameter(
                "@isActive",
                request.IsActive.Value);
        }

        if (request.MinimumPrice.HasValue)
        {
            conditions.Add(
                "c.price >= @minimumPrice");

            query.WithParameter(
                "@minimumPrice",
                request.MinimumPrice.Value);
        }

        if (request.MaximumPrice.HasValue)
        {
            conditions.Add(
                "c.price <= @maximumPrice");

            query.WithParameter(
                "@maximumPrice",
                request.MaximumPrice.Value);
        }

        var sql =
            """
            SELECT *
            FROM c
            """;

        if (conditions.Count > 0)
        {
            sql += Environment.NewLine;

            sql += "WHERE ";

            sql += string.Join(
                Environment.NewLine + "AND ",
                conditions);
        }

        /*
         * IMPORTANT:
         *
         * A deterministic ORDER BY is strongly recommended
         * when demonstrating pagination.
         *
         * Without a stable ordering, the logical sequence
         * of results can be difficult to reason about.
         */

        sql += Environment.NewLine;

        sql += "ORDER BY c.id";

        var finalQuery =
            new QueryDefinition(sql);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            finalQuery.WithParameter(
                "@category",
                request.Category);
        }

        if (request.IsActive.HasValue)
        {
            finalQuery.WithParameter(
                "@isActive",
                request.IsActive.Value);
        }

        if (request.MinimumPrice.HasValue)
        {
            finalQuery.WithParameter(
                "@minimumPrice",
                request.MinimumPrice.Value);
        }

        if (request.MaximumPrice.HasValue)
        {
            finalQuery.WithParameter(
                "@maximumPrice",
                request.MaximumPrice.Value);
        }

        return finalQuery;
    }
}