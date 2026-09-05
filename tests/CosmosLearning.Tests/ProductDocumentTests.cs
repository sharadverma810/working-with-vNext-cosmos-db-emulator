using CosmosLearning.Api.Models;
using Newtonsoft.Json.Linq;

namespace CosmosLearning.Tests;

public sealed class ProductDocumentTests
{
    [Fact]
    public void ProductDocument_serializes_required_nested_and_nullable_fields()
    {
        var document = new ProductDocument
        {
            Id = "product-001",
            ProductName = "Wireless Headphones",
            Category = "Electronics",
            Price = 129.99m,
            Rating = 4.5m,
            CreatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00+00:00"),
            AvailableFrom = null,
            Tags = ["wireless", "audio"],
            Manufacturer = new Manufacturer
            {
                Name = "Northwind Audio",
                Country = "United States"
            },
            Reviews =
            [
                new ProductReview
                {
                    ReviewerName = "Alex",
                    Rating = 5,
                    Comment = "Clear sound.",
                    ReviewDate = DateTimeOffset.Parse("2026-02-01T00:00:00+00:00")
                }
            ]
        };

        var json = JObject.FromObject(document);

        Assert.Equal("product-001", json["id"]?.Value<string>());
        Assert.Equal(129.99m, json["price"]?.Value<decimal>());
        Assert.Null(json["availableFrom"]?.Value<DateTimeOffset?>());
        Assert.Equal("United States", json["manufacturer"]?["country"]?.Value<string>());
        Assert.Equal("Clear sound.", json["reviews"]?[0]?["comment"]?.Value<string>());
    }

    [Fact]
    public void ProductDocument_initializes_collections_and_nested_objects()
    {
        var document = new ProductDocument();

        Assert.NotNull(document.Tags);
        Assert.NotNull(document.SupportedCountries);
        Assert.NotNull(document.RelatedProducts);
        Assert.NotNull(document.Manufacturer);
        Assert.NotNull(document.WarehouseLocation);
        Assert.NotNull(document.Metadata);
        Assert.NotNull(document.Audit);
        Assert.NotNull(document.Reviews);
    }
}