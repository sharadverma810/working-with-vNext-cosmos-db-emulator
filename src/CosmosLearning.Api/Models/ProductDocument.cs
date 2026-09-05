using Newtonsoft.Json;

namespace CosmosLearning.Api.Models;

public sealed class ProductDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    [JsonProperty("subCategory")]
    public string SubCategory { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("discount")]
    public decimal Discount { get; set; }

    [JsonProperty("rating")]
    public decimal Rating { get; set; }

    [JsonProperty("stockQuantity")]
    public int StockQuantity { get; set; }

    [JsonProperty("isActive")]
    public bool IsActive { get; set; }

    [JsonProperty("isFeatured")]
    public bool IsFeatured { get; set; }

    [JsonProperty("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("availableFrom")]
    public DateTimeOffset? AvailableFrom { get; set; }

    [JsonProperty("warrantyDetails")]
    public string? WarrantyDetails { get; set; }

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonProperty("supportedCountries")]
    public List<string> SupportedCountries { get; set; } = [];

    [JsonProperty("relatedProducts")]
    public List<string> RelatedProducts { get; set; } = [];

    [JsonProperty("manufacturer")]
    public Manufacturer Manufacturer { get; set; } = new();

    [JsonProperty("warehouseLocation")]
    public WarehouseLocation WarehouseLocation { get; set; } = new();

    [JsonProperty("metadata")]
    public ProductMetadata Metadata { get; set; } = new();

    [JsonProperty("audit")]
    public AuditInfo Audit { get; set; } = new();

    [JsonProperty("reviews")]
    public List<ProductReview> Reviews { get; set; } = [];
}

public sealed class Manufacturer
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("website")]
    public string? Website { get; set; }
}

public sealed class WarehouseLocation
{
    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; } = string.Empty;
}

public sealed class ProductMetadata
{
    [JsonProperty("source")]
    public string Source { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("importedAt")]
    public DateTimeOffset ImportedAt { get; set; }
}

public sealed class AuditInfo
{
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [JsonProperty("updatedBy")]
    public string? UpdatedBy { get; set; }
}

public sealed class ProductReview
{
    [JsonProperty("reviewerName")]
    public string ReviewerName { get; set; } = string.Empty;

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonProperty("reviewDate")]
    public DateTimeOffset ReviewDate { get; set; }
}