using CosmosLearning.Api.Models;
using CosmosLearning.Api.Services;

namespace CosmosLearning.Tests;

public sealed class ProductQueryBuilderTests
{
    [Fact]
    public void TryBuild_parameterizes_filter_values()
    {
        var request = new ProductQueryRequest
        {
            Category = "Electronics",
            MinimumPrice = 100,
            MaximumPrice = 500,
            Tags = ["wireless"]
        };

        var built = ProductQueryBuilder.TryBuild(request, out var query, out var error);

        Assert.True(built);
        Assert.Null(error);
        Assert.NotNull(query);
        Assert.Contains("c.category = @category", query!.QueryText);
        Assert.Contains("c.price >= @minimumPrice", query.QueryText);
        Assert.Contains("ARRAY_CONTAINS(c.tags, @tag0)", query.QueryText);
        Assert.DoesNotContain("Electronics", query.QueryText);
    }

    [Fact]
    public void TryGetSort_rejects_arbitrary_sort_fields()
    {
        var request = new ProductQueryRequest
        {
            SortField = "price DESC, c.id"
        };

        var built = ProductQueryBuilder.TryGetSort(request, out _, out _, out var error);

        Assert.False(built);
        Assert.Equal("SortField must be one of: price, rating, createdAt, productName.", error);
    }

    [Fact]
    public void TryBuild_allows_only_the_supported_sort_direction()
    {
        var request = new ProductQueryRequest
        {
            SortField = "price",
            SortDirection = "DESC"
        };

        var built = ProductQueryBuilder.TryBuild(request, out var query, out var error);

        Assert.True(built);
        Assert.Null(error);
        Assert.EndsWith("ORDER BY c.price DESC", query!.QueryText);
    }

    [Fact]
    public void TryBuild_supports_nested_objects_arrays_nested_arrays_and_nulls()
    {
        var request = new ProductQueryRequest
        {
            ManufacturerCountry = "United States",
            MetadataSource = "catalog-import",
            SupportedCountry = "Canada",
            ReviewMinimumRating = 4,
            WarrantyIsNull = true,
            WarrantyIsMissing = false
        };

        var built = ProductQueryBuilder.TryBuild(request, out var query, out var error);

        Assert.True(built);
        Assert.Null(error);
        Assert.Contains("c.manufacturer.country = @manufacturerCountry", query!.QueryText);
        Assert.Contains("c.metadata.source = @metadataSource", query.QueryText);
        Assert.Contains("ARRAY_CONTAINS(c.supportedCountries, @supportedCountry)", query.QueryText);
        Assert.Contains("EXISTS (SELECT VALUE r FROM r IN c.reviews", query.QueryText);
        Assert.Contains("IS_NULL(c.warrantyDetails)", query.QueryText);
        Assert.Contains("IS_DEFINED(c.warrantyDetails)", query.QueryText);
    }
}