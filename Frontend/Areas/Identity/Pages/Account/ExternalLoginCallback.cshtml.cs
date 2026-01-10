using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DTOs; // där LoginResponse ligger

namespace SarasBlogg.Areas.Identity.Pages.Account;

public class ExternalLoginCallbackModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _cfg;

    public ExternalLoginCallbackModel(
        IHttpClientFactory httpClientFactory,
        IConfiguration cfg)
    {
        _httpClientFactory = httpClientFactory;
        _cfg = cfg;
    }

    public async Task<IActionResult> OnGet([FromQuery] string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Redirect("/Identity/Account/Login?error=external");

        var apiBase = _cfg["Api:BaseUrl"] ?? "https://localhost:5003";

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(apiBase);

        var response = await client.PostAsJsonAsync(
            "/api/auth/external/exchange",
            new { code }
        );

        if (!response.IsSuccessStatusCode)
            return Redirect("/Identity/Account/Login?error=external");

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (login is null)
            return Redirect("/Identity/Account/Login?error=external");

        // 🔐 Sätt access-token cookie (samma som vanlig login)
        Response.Cookies.Append(
            "api_access_token",
            login.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = login.AccessTokenExpiresUtc
            });

        return Redirect("/");
    }
}