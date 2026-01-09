using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SarasBloggAPI.Controllers;

[ApiController]
[Route("api/auth/external")]
[Produces("application/json")]
public class ExternalAuthController : ControllerBase
{
    private readonly ILogger<ExternalAuthController> _logger;

    public ExternalAuthController(ILogger<ExternalAuthController> logger)
    {
        _logger = logger;
    }

    // ---------- GOOGLE: START ----------
    [AllowAnonymous]
    [HttpGet("google")]
    public IActionResult Google()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // ---------- GOOGLE: CALLBACK ----------
    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(
            GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded || result.Principal is null)
        {
            _logger.LogWarning("Google external login failed");
            return Unauthorized(new { error = "External login failed" });
        }

        var claims = result.Principal.Claims
            .Select(c => new { c.Type, c.Value });

        _logger.LogInformation("Google login successful");

        return Ok(new
        {
            Provider = "Google",
            Claims = claims
        });
    }
}