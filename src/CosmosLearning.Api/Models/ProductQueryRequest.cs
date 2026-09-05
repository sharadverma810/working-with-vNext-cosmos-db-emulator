using System.ComponentModel.DataAnnotations;

namespace CosmosLearning.Api.Models;

public sealed class ProductQueryRequest : IValidatableObject
{
    public string? Category { get; init; }

    public string? SubCategory { get; init; }

    public string? Status { get; init; }

    public decimal? MinimumPrice { get; init; }

    public decimal? MaximumPrice { get; init; }

    public decimal? MinimumRating { get; init; }

    public bool? IsActive { get; init; }

    public bool? IsFeatured { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public string? City { get; init; }

    public string? ManufacturerCountry { get; init; }

    public string? MetadataSource { get; init; }

    public string? SupportedCountry { get; init; }

    public int? ReviewMinimumRating { get; init; }

    public bool? WarrantyIsNull { get; init; }

    public bool? WarrantyIsMissing { get; init; }

    public List<string> Tags { get; init; } = [];

    public string? SortField { get; init; }

    public string? SortDirection { get; init; }

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    public string? ContinuationToken { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinimumPrice is < 0)
        {
            yield return new ValidationResult("MinimumPrice cannot be negative.", [nameof(MinimumPrice)]);
        }

        if (MaximumPrice is < 0)
        {
            yield return new ValidationResult("MaximumPrice cannot be negative.", [nameof(MaximumPrice)]);
        }

        if (MinimumPrice is not null && MaximumPrice is not null && MinimumPrice > MaximumPrice)
        {
            yield return new ValidationResult(
                "MinimumPrice cannot be greater than MaximumPrice.",
                [nameof(MinimumPrice), nameof(MaximumPrice)]);
        }

        if (MinimumRating is < 0 or > 5)
        {
            yield return new ValidationResult("MinimumRating must be between 0 and 5.", [nameof(MinimumRating)]);
        }

        if (ReviewMinimumRating is < 1 or > 5)
        {
            yield return new ValidationResult(
                "ReviewMinimumRating must be between 1 and 5.",
                [nameof(ReviewMinimumRating)]);
        }

        if (CreatedAfter is not null && CreatedBefore is not null && CreatedAfter > CreatedBefore)
        {
            yield return new ValidationResult(
                "CreatedAfter cannot be later than CreatedBefore.",
                [nameof(CreatedAfter), nameof(CreatedBefore)]);
        }

        if (Tags.Any(tag => string.IsNullOrWhiteSpace(tag)))
        {
            yield return new ValidationResult("Tags cannot contain empty values.", [nameof(Tags)]);
        }
    }
}