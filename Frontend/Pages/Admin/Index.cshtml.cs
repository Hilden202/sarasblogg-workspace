using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SarasBlogg.DAL;
using SarasBlogg.DTOs;
using SarasBlogg.Models;
using SarasBlogg.Extensions; // ToSwedishTime (används i listan)
using SarasBlogg.Services;   // BloggService för cache-invalidering

namespace SarasBlogg.Pages.Admin
{
    [Authorize(Roles = "admin, superadmin, superuser")]
    public class IndexModel : PageModel
    {
        // API-tjänster för datahantering
        private readonly BloggAPIManager _bloggApi;
        private readonly BloggImageAPIManager _imageApi;
        private readonly CommentAPIManager _commentApi;

        // Cache-tjänst (publik listcache)
        private readonly BloggService _bloggService;

        public IndexModel(
            BloggAPIManager bloggApi,
            BloggImageAPIManager imageApi,
            CommentAPIManager commentApi,
            BloggService bloggService)
        {
            _bloggApi = bloggApi;
            _imageApi = imageApi;
            _commentApi = commentApi;
            _bloggService = bloggService;
        }

        public List<BloggWithImage> BloggsWithImage { get; set; } = new();

        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsSuperUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int? hiddenId, int? archiveId)
        {
            // roles
            IsAdmin = User.IsInRole("admin");
            IsSuperAdmin = User.IsInRole("superadmin");
            IsSuperUser = User.IsInRole("superuser");

            // ---- Toggle HIDDEN (admin + superadmin) ----
            if ((IsAdmin || IsSuperAdmin) && hiddenId is int hid and > 0)
            {
                await _bloggApi.ToggleHiddenAsync(hid);
                _bloggService.InvalidateBlogListCache();
                return RedirectToPage();
            }

            // ---- Toggle ARCHIVED (admin + superadmin) ----
            if ((IsAdmin || IsSuperAdmin) && archiveId is int aid and > 0)
            {
                await _bloggApi.ToggleArchivedAsync(aid);
                _bloggService.InvalidateBlogListCache();
                return RedirectToPage();
            }

            // load lists
            await LoadBloggsWithImagesAsync();

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteBloggAsync(int deleteId)
        {
            if (!User.IsInRole("superadmin")) return Forbid();

            var bloggToDelete = await _bloggApi.GetBloggAsync(deleteId);
            if (bloggToDelete != null)
            {
                await _commentApi.DeleteCommentsAsync(bloggToDelete.Id);
                await _imageApi.DeleteImagesByBloggIdAsync(bloggToDelete.Id);
                await _bloggApi.DeleteBloggAsync(bloggToDelete.Id);
                _bloggService.InvalidateBlogListCache();
            }

            return RedirectToPage();
        }

        private async Task LoadBloggsWithImagesAsync()
        {
            var allBloggs = await _bloggApi.GetAllBloggsAsync();
            BloggsWithImage = new List<BloggWithImage>();

            foreach (var blogg in allBloggs)
            {
                var images = await _imageApi.GetImagesByBloggIdAsync(blogg.Id);

                BloggsWithImage.Add(new BloggWithImage
                {
                    Blogg = blogg,
                    Images = images
                });
            }
        }
    }
}
