using CosmosLearning.Api.Features.Products.Pagination.Contracts;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Features.Products.Pagination;

public static class ProductPaginationQueryBuilder
{
    public static QueryDefinition Build(ProductPageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            conditions.Add(
                "c.category = @category");
        }

        if (request.IsActive.HasValue)
        {
            conditions.Add(
                "c.isActive = @isActive");
        }

        if (request.MinimumPrice.HasValue)
        {
            conditions.Add(
                "c.price >= @minimumPrice");
        }

        if (request.MaximumPrice.HasValue)
        {
            conditions.Add(
                "c.price <= @maximumPrice");
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

        // A deterministic order makes pagination
        // easier to understand and test.
        sql += Environment.NewLine;

        sql += "ORDER BY c.id";

        var query = new QueryDefinition(sql);

        AddParameters(query, request);

        return query;
    }

    private static void AddParameters(
        QueryDefinition query,
        ProductPageRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query.WithParameter(
                "@category",
                request.Category);
        }

        if (request.IsActive.HasValue)
        {
            query.WithParameter(
                "@isActive",
                request.IsActive.Value);
        }

        if (request.MinimumPrice.HasValue)
        {
            query.WithParameter(
                "@minimumPrice",
                request.MinimumPrice.Value);
        }

        if (request.MaximumPrice.HasValue)
        {
            query.WithParameter(
                "@maximumPrice",
                request.MaximumPrice.Value);
        }
    }
}