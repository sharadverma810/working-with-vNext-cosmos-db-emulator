using CosmosLearning.Api.Models;

namespace CosmosLearning.Bootstrap;

public sealed class ProductDataGenerator
{
    private static readonly string[] Categories = ["Electronics", "Home", "Outdoor", "Books", "Fitness"];
    private static readonly string[] SubCategories = ["Audio", "Kitchen", "Camping", "Fiction", "Training"];
    private static readonly string[] Statuses = ["Active", "Draft", "Discontinued", "Backorder"];
    private static readonly string[] Countries = ["United States", "Canada", "United Kingdom", "Germany", "Australia"];
    private static readonly string[] Cities = ["Seattle", "Toronto", "London", "Berlin", "Melbourne"];
    private static readonly string[] Tags = ["premium", "wireless", "compact", "eco-friendly", "waterproof", "beginner"];
    private static readonly string[] Manufacturers = ["Northwind", "Contoso", "Adventure Works", "Fabrikam", "Tailspin"];

    private readonly Random random;

    public ProductDataGenerator(int seed = 20260905)
    {
        random = new Random(seed);
    }

    public IReadOnlyList<ProductDocument> Generate(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var products = new List<ProductDocument>(count);
        var startDate = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var index = 1; index <= count; index++)
        {
            var category = Pick(Categories);
            var createdAt = startDate.AddDays(random.Next(0, 1_200)).AddMinutes(random.Next(0, 1_440));
            products.Add(new ProductDocument
            {
                Id = $"product-{index:00000}",
                ProductName = $"{Pick(Manufacturers)} {Pick(SubCategories)} {index:00000}",
                Description = $"A sample {category.ToLowerInvariant()} catalog item for Cosmos DB query learning.",
                Category = category,
                SubCategory = Pick(SubCategories),
                Status = Pick(Statuses),
                Price = Math.Round((decimal)(random.NextDouble() * 1_950 + 50), 2),
                Discount = Math.Round((decimal)(random.NextDouble() * 35), 2),
                Rating = Math.Round((decimal)(random.NextDouble() * 4 + 1), 1),
                StockQuantity = random.Next(0, 1_000),
                IsActive = random.Next(0, 100) >= 12,
                IsFeatured = random.Next(0, 100) < 20,
                IsAvailable = random.Next(0, 100) >= 18,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddDays(random.Next(0, 180)),
                AvailableFrom = random.Next(0, 100) < 15 ? null : createdAt.AddDays(random.Next(1, 90)),
                WarrantyDetails = random.Next(0, 100) < 20 ? null : $"{random.Next(1, 4)} year limited warranty",
                Tags = PickMany(Tags, 1, 3),
                SupportedCountries = PickMany(Countries, 1, 4),
                RelatedProducts = [BuildRelatedProductId(index, count), BuildRelatedProductId(index + 1, count)],
                Manufacturer = new Manufacturer
                {
                    Name = Pick(Manufacturers),
                    Country = Pick(Countries),
                    Website = random.Next(0, 100) < 10 ? null : "https://example.com/manufacturer"
                },
                WarehouseLocation = new WarehouseLocation
                {
                    Country = Pick(Countries),
                    State = $"State-{random.Next(1, 10):00}",
                    City = Pick(Cities),
                    PostalCode = $"{random.Next(10000, 99999)}"
                },
                Metadata = new ProductMetadata
                {
                    Source = Pick(["catalog-import", "manual-entry", "partner-feed"]),
                    Version = $"v{random.Next(1, 4)}.{random.Next(0, 10)}",
                    ImportedAt = createdAt.AddHours(random.Next(1, 72))
                },
                Audit = new AuditInfo
                {
                    CreatedBy = Pick(["seed-generator", "catalog-admin", "partner-import"]),
                    UpdatedBy = random.Next(0, 100) < 12 ? null : Pick(["catalog-admin", "inventory-service"])
                },
                Reviews = GenerateReviews(createdAt)
            });
        }

        return products;
    }

    private List<ProductReview> GenerateReviews(DateTimeOffset createdAt)
    {
        var reviews = new List<ProductReview>();

        for (var index = 0; index < random.Next(0, 4); index++)
        {
            reviews.Add(new ProductReview
            {
                ReviewerName = $"Reviewer-{random.Next(1000, 9999)}",
                Rating = random.Next(1, 6),
                Comment = Pick(["Good value.", "Works as expected.", "Would recommend.", "Needs improvement."]),
                ReviewDate = createdAt.AddDays(random.Next(1, 365))
            });
        }

        return reviews;
    }

    private List<string> PickMany(string[] values, int minimum, int maximum)
    {
        var selected = new HashSet<string>(StringComparer.Ordinal);
        var count = random.Next(minimum, maximum + 1);

        while (selected.Count < count)
        {
            selected.Add(Pick(values));
        }

        return [.. selected];
    }

    private string BuildRelatedProductId(int index, int count)
    {
        var relatedIndex = ((index - 1 + random.Next(1, 20)) % count) + 1;
        return $"product-{relatedIndex:00000}";
    }

    private string Pick(string[] values) => values[random.Next(values.Length)];
}