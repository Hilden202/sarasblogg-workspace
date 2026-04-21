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
        try
        {
            if (request == null || request.Cards == null || !request.Cards.Any())
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    message = "No cards provided."
                });
            }

            Console.WriteLine($"[Tarot] Interpret called - Cards: {request.Cards.Count}, Lang: {request.Language}, Mode: {request.Mode ?? "soft"}");

            var result = await _tarotService.InterpretAsync(request);

            if (string.IsNullOrWhiteSpace(result))
            {
                Console.WriteLine("[Tarot] Interpretation returned empty");

                return StatusCode(500, new
                {
                    error = "interpretation_failed",
                    message = "Could not generate interpretation."
                });
            }

            Console.WriteLine("[Tarot] Interpretation success");

            return Ok(new TarotInterpretResponse
            {
                Interpretation = result
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Tarot] ERROR: {ex.Message}");

            return StatusCode(500, new
            {
                error = "server_error",
                message = "Something went wrong while generating interpretation."
            });
        }
    }
}