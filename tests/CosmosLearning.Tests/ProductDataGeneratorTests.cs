using CosmosLearning.Bootstrap;

namespace CosmosLearning.Tests;

public sealed class ProductDataGeneratorTests
{
    [Fact]
    public void Generate_is_deterministic_for_the_same_seed()
    {
        var first = new ProductDataGenerator(42).Generate(10);
        var second = new ProductDataGenerator(42).Generate(10);

        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(first),
            System.Text.Json.JsonSerializer.Serialize(second));
    }

    [Fact]
    public void Generate_produces_variety_and_nullable_values()
    {
        var products = new ProductDataGenerator(42).Generate(500);

        Assert.True(products.Select(product => product.Category).Distinct().Count() >= 4);
        Assert.True(products.Select(product => product.Status).Distinct().Count() >= 3);
        Assert.Contains(products, product => product.AvailableFrom is null);
        Assert.Contains(products, product => product.WarrantyDetails is null);
        Assert.Contains(products, product => product.Reviews.Count > 0);
        Assert.Contains(products, product => product.Tags.Count > 1);
    }
}