using CosmosLearning.Api.Features.Products.Pagination.Contracts;

namespace CosmosLearning.Api.Features.Products.Pagination;

public static class ProductPaginationValidator
{
    public const int MinimumPageSize = 1;

    public const int MaximumPageSize = 100;

    public static bool TryValidate(
        ProductPageRequest request,
        out string? error)
    {
        if (request.PageSize < MinimumPageSize)
        {
            error =
                $"PageSize must be at least {MinimumPageSize}.";

            return false;
        }

        if (request.PageSize > MaximumPageSize)
        {
            error =
                $"PageSize cannot exceed {MaximumPageSize}.";

            return false;
        }

        if (request.MinimumPrice.HasValue
            && request.MaximumPrice.HasValue
            && request.MinimumPrice > request.MaximumPrice)
        {
            error =
                "MinimumPrice cannot be greater than MaximumPrice.";

            return false;
        }

        error = null;

        return true;
    }
}