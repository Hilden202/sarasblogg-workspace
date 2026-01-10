using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DAL;
using SarasBlogg.Services;

namespace SarasBlogg.Areas.Identity.Pages.Account;

public class ExternalLoginCallbackModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAccessTokenStore _tokenStore;
    private readonly IConfiguration _config;

    public ExternalLoginCallbackModel(
        IHttpClientFactory httpClientFactory,
        IAccessTokenStore tokenStore,
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _config = config;
    }

    public async Task<IActionResult> OnGet([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Redirect("/Identity/Account/Login?error=external");

        // 🌍 API base
        var apiBase = _config["ApiSettings:BaseAddress"]
                      ?? throw new InvalidOperationException("ApiSettings:BaseAddress missing");

        var client = _httpClientFactory.CreateClient();

        // 🔁 1. Exchange code → tokens
        var exchangeResponse = await client.PostAsJsonAsync(
            $"{apiBase}/api/auth/external/exchange",
            new { code });

        if (!exchangeResponse.IsSuccessStatusCode)
            return Redirect("/Identity/Account/Login?error=external");

        var tokens = await exchangeResponse.Content
            .ReadFromJsonAsync<ExternalLoginExchangeResponse>();

        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
            return Redirect("/Identity/Account/Login?error=external");

        // 🔐 2. Läs claims från JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokens.AccessToken);

        var identity = new ClaimsIdentity(
            jwt.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        // 🍪 3. Skapa auth-cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = tokens.AccessTokenExpiresUtc
            });

        // 🧠 4. Spara token lokalt (för API-anrop)
        _tokenStore.Set(tokens.AccessToken);

        // 🔑 5. Lägg token-cookie för API
        Response.Cookies.Append(
            "api_access_token",
            tokens.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = tokens.AccessTokenExpiresUtc
            });

        // 🔎 6. Kolla om användaren måste sätta användarnamn
        var meRequest = new HttpRequestMessage(HttpMethod.Get, $"{apiBase}/api/users/me");
        meRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", tokens.AccessToken);

        using var meResponse = await client.SendAsync(meRequest);

        if (!meResponse.IsSuccessStatusCode)
            return Redirect("/");

        var contentType = meResponse.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase))
            return Redirect("/");

        var me = await meResponse.Content.ReadFromJsonAsync<MeResponse>();
        if (me?.RequiresUsernameSetup == true)
            return RedirectToPage("/Account/SetUsername");

        // 🚀 7. Klar
        
        return Redirect("/Identity/Account/Manage/Index");
    }

    // DTO som matchar API:ts exchange-response
    private sealed class ExternalLoginExchangeResponse
    {
        public string AccessToken { get; init; } = "";
        public DateTime AccessTokenExpiresUtc { get; init; }
    }
    
    private sealed class MeResponse
    {
        public bool RequiresUsernameSetup { get; init; }
    }
    
}