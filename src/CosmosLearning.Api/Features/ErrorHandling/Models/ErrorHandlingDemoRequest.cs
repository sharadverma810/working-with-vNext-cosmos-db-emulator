using System.ComponentModel.DataAnnotations;

namespace CosmosLearning.Api.Features.ErrorHandling.Models;

public sealed class ErrorHandlingDemoRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string? Name { get; init; }

    [Range(1, 100)]
    public int Quantity { get; init; }
}