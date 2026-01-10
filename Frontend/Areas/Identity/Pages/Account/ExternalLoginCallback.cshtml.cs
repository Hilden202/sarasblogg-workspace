using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SarasBlogg.Areas.Identity.Pages.Account;

public class ExternalLoginCallbackModel : PageModel
{
    public IActionResult OnGet(
        [FromQuery] string accessToken,
        [FromQuery] string refreshToken,
        [FromQuery] string accessTokenExpiresUtc)
    {
        // 🔴 Skydd: callback nådd men utan token
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Redirect("/Identity/Account/Login?error=external");
        }

        // 🔴 PARSA SÄKERT
        if (!DateTimeOffset.TryParse(
                accessTokenExpiresUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var expiresOffset))
        {
            return Redirect("/Identity/Account/Login?error=token-expiry");
        }

        var expiresUtc = expiresOffset.UtcDateTime;

        // 🔐 Sätt API-token i cookie
        Response.Cookies.Append(
            "api_access_token",
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, // localhost → false, prod → true
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = expiresUtc
            });

        return Page();
    }
}