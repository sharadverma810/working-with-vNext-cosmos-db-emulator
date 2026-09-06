using CosmosLearning.Api.Features.ErrorHandling.Models;
using CosmosLearning.Api.Features.ErrorHandling.Services;
using Microsoft.AspNetCore.Mvc;

namespace CosmosLearning.Api.Features.ErrorHandling;

[ApiController]
[Route("error-handling")]
public sealed class ErrorHandlingController(
    ErrorHandlingDemoService service)
    : ControllerBase
{
    [HttpGet("success")]
    public async Task<ActionResult<ErrorHandlingDemoResponse>> Success(
        CancellationToken cancellationToken)
    {
        var traceId =
            HttpContext.TraceIdentifier;

        var response =
            await service.SuccessAsync(traceId);

        return Ok(response);
    }

    [HttpPost("validation")]
    public IActionResult Validation(
        ErrorHandlingDemoRequest request)
    {
        return Ok(new
        {
            message = "Validation succeeded.",
            name = request.Name,
            quantity = request.Quantity
        });
    }

    [HttpGet("cosmos/not-found")]
    public async Task<IActionResult> CosmosNotFound(
        CancellationToken cancellationToken)
    {
        await service.SimulateNotFoundAsync(
            cancellationToken);

        return NoContent();
    }

    [HttpPost("cosmos/conflict")]
    public async Task<IActionResult> CosmosConflict(
        CancellationToken cancellationToken)
    {
        await service.SimulateConflictAsync(
            cancellationToken);

        return NoContent();
    }

    [HttpGet("cosmos/throttling")]
    public async Task<IActionResult> CosmosThrottling()
    {
        await service.SimulateThrottlingAsync();

        return NoContent();
    }

    [HttpGet("cosmos/unavailable")]
    public async Task<IActionResult> CosmosUnavailable()
    {
        await service.SimulateUnavailableAsync();

        return NoContent();
    }

    [HttpGet("unexpected")]
    public async Task<IActionResult> Unexpected()
    {
        await service.SimulateUnexpectedFailureAsync();

        return NoContent();
    }

    [HttpGet("cancellation")]
    public async Task<IActionResult> Cancellation(
        CancellationToken cancellationToken)
    {
        await service.SimulateCancellationAsync(
            cancellationToken);

        return Ok(new
        {
            message = "The operation completed successfully."
        });
    }
}