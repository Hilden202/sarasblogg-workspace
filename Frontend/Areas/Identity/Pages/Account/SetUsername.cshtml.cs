using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DAL;

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

        public class InputModel
        {
            [Required]
            [MinLength(3)]
            [Display(Name = "Användarnamn")]
            public string UserName { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var me = await _userApi.GetMeAsync();

            if (me == null)
                return RedirectToPage("/Account/Login");

            if (!me.RequiresUsernameSetup)
                return RedirectToPage("/Account/Manage/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
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
            await _userApi.RefreshSessionAsync();

            // 3️⃣ Klart → tillbaka till profilen
            return RedirectToPage("/Account/Manage/Index");
        }
    }
}