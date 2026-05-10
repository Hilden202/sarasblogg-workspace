using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SarasBloggAPI.DTOs.Guidance;
using SarasBloggAPI.Services.Guidance;

namespace SarasBloggAPI.Controllers.Guidance;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("guidance")]
public class GuidanceController : ControllerBase
{
    private readonly GuidanceService _guidanceService;
    private readonly ILogger<GuidanceController> _logger;

    public GuidanceController(GuidanceService guidanceService, ILogger<GuidanceController> logger)
    {
        _guidanceService = guidanceService;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<GuidanceResponse>> Create(
        [FromBody] GuidanceRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest(new
            {
                error = "invalid_request",
                message = "Skriv ett ord eller en kort fråga."
            });
        }

        try
        {
            var result = await _guidanceService.GenerateAsync(request, cancellationToken);

            if (string.IsNullOrWhiteSpace(result.Guidance))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "guidance_unavailable",
                    message = "Vägledningen kunde inte hämtas just nu. Försök igen om en stund."
                });
            }

            return Ok(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Could not generate footer guidance.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "guidance_unavailable",
                message = "Vägledningen kunde inte hämtas just nu. Försök igen om en stund."
            });
        }
    }
}
