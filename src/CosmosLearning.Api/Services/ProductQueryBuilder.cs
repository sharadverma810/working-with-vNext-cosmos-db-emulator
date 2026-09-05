using CosmosLearning.Api.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosLearning.Api.Services;

public static class ProductQueryBuilder
{
    private static readonly IReadOnlyDictionary<string, string> SortFields =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["price"] = "c.price",
            ["rating"] = "c.rating",
            ["createdAt"] = "c.createdAt",
            ["productName"] = "c.productName"
        };

    public static bool TryBuild(
        ProductQueryRequest request,
        out QueryDefinition? query,
        out string? error)
    {
        var predicates = new List<string>();
        var definition = new QueryDefinition("SELECT * FROM c");

        if (!AddStringFilter(request.Category, "category", "@category", predicates, ref definition, out error)
            || !AddStringFilter(request.SubCategory, "subCategory", "@subCategory", predicates, ref definition, out error)
            || !AddStringFilter(request.Status, "status", "@status", predicates, ref definition, out error))
        {
            query = null;
            return false;
        }

        if (request.MinimumPrice is not null)
        {
            predicates.Add("c.price >= @minimumPrice");
            definition = definition.WithParameter("@minimumPrice", request.MinimumPrice.Value);
        }

        if (request.MaximumPrice is not null)
        {
            predicates.Add("c.price <= @maximumPrice");
            definition = definition.WithParameter("@maximumPrice", request.MaximumPrice.Value);
        }

        if (request.MinimumRating is not null)
        {
            predicates.Add("c.rating >= @minimumRating");
            definition = definition.WithParameter("@minimumRating", request.MinimumRating.Value);
        }

        if (request.IsActive is not null)
        {
            predicates.Add("c.isActive = @isActive");
            definition = definition.WithParameter("@isActive", request.IsActive.Value);
        }

        if (request.IsFeatured is not null)
        {
            predicates.Add("c.isFeatured = @isFeatured");
            definition = definition.WithParameter("@isFeatured", request.IsFeatured.Value);
        }

        if (request.CreatedAfter is not null)
        {
            predicates.Add("c.createdAt >= @createdAfter");
            definition = definition.WithParameter("@createdAfter", request.CreatedAfter.Value);
        }

        if (request.CreatedBefore is not null)
        {
            predicates.Add("c.createdAt <= @createdBefore");
            definition = definition.WithParameter("@createdBefore", request.CreatedBefore.Value);
        }

        if (!AddStringFilter(request.City, "warehouseLocation.city", "@city", predicates, ref definition, out error))
        {
            query = null;
            return false;
        }

        if (!AddStringFilter(request.ManufacturerCountry, "manufacturer.country", "@manufacturerCountry", predicates, ref definition, out error)
            || !AddStringFilter(request.MetadataSource, "metadata.source", "@metadataSource", predicates, ref definition, out error))
        {
            query = null;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.SupportedCountry))
        {
            predicates.Add("ARRAY_CONTAINS(c.supportedCountries, @supportedCountry)");
            definition = definition.WithParameter("@supportedCountry", request.SupportedCountry);
        }

        if (request.ReviewMinimumRating is not null)
        {
            predicates.Add("EXISTS (SELECT VALUE r FROM r IN c.reviews WHERE r.rating >= @reviewMinimumRating)");
            definition = definition.WithParameter("@reviewMinimumRating", request.ReviewMinimumRating.Value);
        }

        if (request.WarrantyIsNull is not null)
        {
            predicates.Add(request.WarrantyIsNull.Value
                ? "IS_NULL(c.warrantyDetails)"
                : "NOT IS_NULL(c.warrantyDetails)");
        }

        if (request.WarrantyIsMissing is not null)
        {
            predicates.Add(request.WarrantyIsMissing.Value
                ? "NOT IS_DEFINED(c.warrantyDetails)"
                : "IS_DEFINED(c.warrantyDetails)");
        }

        for (var index = 0; index < request.Tags.Count; index++)
        {
            var parameterName = $"@tag{index}";
            predicates.Add($"ARRAY_CONTAINS(c.tags, {parameterName})");
            definition = definition.WithParameter(parameterName, request.Tags[index]);
        }

        if (predicates.Count > 0)
        {
            definition = new QueryDefinition(
                $"SELECT * FROM c WHERE {string.Join(" AND ", predicates)}");

            AddParameters(request, ref definition);
        }

        if (!TryGetSort(request, out var sortExpression, out var sortDirection, out error))
        {
            query = null;
            return false;
        }

        if (sortExpression is not null)
        {
            definition = definition.WithParameter("@sortField", sortExpression);
            definition = new QueryDefinition(
                $"{GetQueryText(definition)} ORDER BY {sortExpression} {sortDirection}");
            AddParameters(request, ref definition);
        }

        query = definition;
        return true;
    }

    public static bool TryGetSort(
        ProductQueryRequest request,
        out string? sortExpression,
        out string sortDirection,
        out string? error)
    {
        sortExpression = null;
        sortDirection = "ASC";
        error = null;

        if (string.IsNullOrWhiteSpace(request.SortField))
        {
            return true;
        }

        if (!SortFields.TryGetValue(request.SortField, out sortExpression))
        {
            error = "SortField must be one of: price, rating, createdAt, productName.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.SortDirection)
            && !request.SortDirection.Equals("ASC", StringComparison.OrdinalIgnoreCase)
            && !request.SortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase))
        {
            error = "SortDirection must be ASC or DESC.";
            return false;
        }

        sortDirection = string.Equals(request.SortDirection, "DESC", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";
        return true;
    }

    private static bool AddStringFilter(
        string? value,
        string path,
        string parameterName,
        List<string> predicates,
        ref QueryDefinition definition,
        out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        predicates.Add($"c.{path} = {parameterName}");
        definition = definition.WithParameter(parameterName, value);
        return true;
    }

    private static void AddParameters(ProductQueryRequest request, ref QueryDefinition definition)
    {
        if (request.MinimumPrice is not null)
        {
            definition = definition.WithParameter("@minimumPrice", request.MinimumPrice.Value);
        }

        if (request.MaximumPrice is not null)
        {
            definition = definition.WithParameter("@maximumPrice", request.MaximumPrice.Value);
        }

        if (request.MinimumRating is not null)
        {
            definition = definition.WithParameter("@minimumRating", request.MinimumRating.Value);
        }

        if (request.IsActive is not null)
        {
            definition = definition.WithParameter("@isActive", request.IsActive.Value);
        }

        if (request.IsFeatured is not null)
        {
            definition = definition.WithParameter("@isFeatured", request.IsFeatured.Value);
        }

        if (request.CreatedAfter is not null)
        {
            definition = definition.WithParameter("@createdAfter", request.CreatedAfter.Value);
        }

        if (request.CreatedBefore is not null)
        {
            definition = definition.WithParameter("@createdBefore", request.CreatedBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            definition = definition.WithParameter("@category", request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.SubCategory))
        {
            definition = definition.WithParameter("@subCategory", request.SubCategory);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            definition = definition.WithParameter("@status", request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            definition = definition.WithParameter("@city", request.City);
        }

        if (!string.IsNullOrWhiteSpace(request.ManufacturerCountry))
        {
            definition = definition.WithParameter("@manufacturerCountry", request.ManufacturerCountry);
        }

        if (!string.IsNullOrWhiteSpace(request.MetadataSource))
        {
            definition = definition.WithParameter("@metadataSource", request.MetadataSource);
        }

        if (!string.IsNullOrWhiteSpace(request.SupportedCountry))
        {
            definition = definition.WithParameter("@supportedCountry", request.SupportedCountry);
        }

        if (request.ReviewMinimumRating is not null)
        {
            definition = definition.WithParameter("@reviewMinimumRating", request.ReviewMinimumRating.Value);
        }

        for (var index = 0; index < request.Tags.Count; index++)
        {
            definition = definition.WithParameter($"@tag{index}", request.Tags[index]);
        }
    }

    private static string GetQueryText(QueryDefinition definition) =>
        definition.QueryText;
}