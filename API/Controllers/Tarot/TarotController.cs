using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SarasBloggAPI.Services.Tarot;
using SarasBloggAPI.DTOs.Tarot;

namespace SarasBloggAPI.Controllers.Tarot;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("tarot")]
public class TarotController : ControllerBase
{
    private readonly TarotService _tarotService;

    public TarotController(TarotService tarotService)
    {
        _tarotService = tarotService;
    }

    [HttpPost("interpret")]
    public async Task<ActionResult<TarotInterpretResponse>> Interpret([FromBody] TarotInterpretRequest request)
    {
        var result = await _tarotService.InterpretAsync(request);

        if (string.IsNullOrWhiteSpace(result))
        {
            return StatusCode(500, new
            {
                error = "interpretation_failed",
                message = "Could not generate interpretation."
            });
        }

        return Ok(new TarotInterpretResponse
        {
            Interpretation = result
        });
    }
}