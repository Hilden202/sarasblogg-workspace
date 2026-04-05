using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DAL;
using SarasBlogg.DTOs;

namespace SarasBlogg.Areas.Identity.Pages.Account
{
    [Authorize]
    public class SetUsernameModel : PageModel
    {
        private readonly UserAPIManager _userApi;

        public SetUsernameModel(UserAPIManager userApi)
        {
            _userApi = userApi;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; private set; }

        public class InputModel
        {
            [Required]
            [MinLength(3)]
            [Display(Name = "Användarnamn")]
            public string UserName { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = NormalizeLocalReturnUrl(returnUrl);
            var me = await _userApi.GetMeAsync();

            if (me == null)
                return RedirectToPage("/Account/Login", new { returnUrl = ReturnUrl });

            if (!me.RequiresUsernameSetup)
                return RedirectToReturnUrlOrDefault(ReturnUrl, "/Identity/Account/Manage/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = NormalizeLocalReturnUrl(returnUrl);

            if (!ModelState.IsValid)
                return Page();

            // 1️⃣ Sätt användarnamn via API
            var res = await _userApi.ChangeMyUserNameAsync(Input.UserName);

            if (res?.Succeeded != true)
            {
                ModelState.AddModelError(
                    string.Empty,
                    res?.Message ?? "Kunde inte spara användarnamn."
                );
                return Page();
            }

            // 2️⃣ 🔥 KRITISK DEL: synka om frontend-sessionen (cookie + claims)
            var refreshedSession = await _userApi.RefreshSessionAsync();
            if (refreshedSession is not null)
            {
                await ApplyRefreshedSessionAsync(refreshedSession);
            }

            // 3️⃣ Klart → tillbaka dit användaren kom ifrån om möjligt
            return RedirectToReturnUrlOrDefault(ReturnUrl, "/Identity/Account/Manage/Index");
        }

        private async Task ApplyRefreshedSessionAsync(AccessTokenDto session)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(session.AccessToken);

            var identity = new ClaimsIdentity(
                jwt.Claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = session.AccessTokenExpiresUtc
                });

            Response.Cookies.Append(
                "api_access_token",
                session.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = session.AccessTokenExpiresUtc
                });
        }

        private IActionResult RedirectToReturnUrlOrDefault(string? returnUrl, string fallbackPath)
        {
            var safeReturnUrl = NormalizeLocalReturnUrl(returnUrl);
            return !string.IsNullOrWhiteSpace(safeReturnUrl)
                ? LocalRedirect(safeReturnUrl)
                : Redirect(fallbackPath);
        }

        private string? NormalizeLocalReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return null;

            return Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        }
    }
}
