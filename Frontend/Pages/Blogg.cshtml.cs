using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SarasBlogg.Services;
using SarasBlogg.DAL;
using SarasBlogg.Models;
using SarasBlogg.Pages.Shared;

namespace SarasBlogg.Pages
{
    public class BloggModel : BloggBasePageModel
    {
        private readonly BloggAPIManager _bloggApi;
        private readonly BloggImageAPIManager _imageApi;

        private static readonly TimeZoneInfo TzSe = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

        public BloggModel(
            BloggService bloggService,
            UserAPIManager userApi,
            BloggAPIManager bloggApi,
            BloggImageAPIManager imageApi)
            : base(bloggService, userApi, isArchive: false)
        {
            _bloggApi = bloggApi;
            _imageApi = imageApi;
            NewBlogg = new Blogg();
        }

        public string? EditorAccessToken { get; private set; }
        public BloggWithImage? EditedBloggWithImages { get; set; }

        [BindProperty]
        public Blogg NewBlogg { get; set; }

        [BindProperty]
        public IFormFile[]? BloggImages { get; set; } = Array.Empty<IFormFile>();

        public bool IsSuperAdmin { get; set; }

        public async Task OnGetAsync(int showId, int id, bool openComments = false, int? editId = null)
        {
            IsSuperAdmin = User.IsInRole("superadmin");

            await OnGetCoreAsync(showId, id, openComments);

            if (NewBlogg.Id == 0 && NewBlogg.LaunchDate == default)
            {
                var todaySe = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TzSe).Date;
                NewBlogg.LaunchDate = DateTime.SpecifyKind(todaySe, DateTimeKind.Unspecified);
            }

            if (IsSuperAdmin)
            {
                EditorAccessToken = await _bloggApi.GetEditorAccessTokenAsync();

                if (editId.HasValue && editId.Value != 0)
                {
                    var blogg = await _bloggApi.GetBloggAsync(editId.Value);
                    if (blogg != null)
                    {
                        var images = await _imageApi.GetImagesByBloggIdAsync(editId.Value);
                        EditedBloggWithImages = new BloggWithImage { Blogg = blogg, Images = images };
                        NewBlogg = new Blogg
                        {
                            Id = blogg.Id,
                            Title = blogg.Title,
                            Content = blogg.Content,
                            Author = blogg.Author,
                            LaunchDate = blogg.LaunchDate,
                            UserId = blogg.UserId
                        };

                        if (NewBlogg.LaunchDate.Kind == DateTimeKind.Utc)
                        {
                            var se = TimeZoneInfo.ConvertTimeFromUtc(NewBlogg.LaunchDate, TzSe).Date;
                            NewBlogg.LaunchDate = DateTime.SpecifyKind(se, DateTimeKind.Unspecified);
                        }
                        else
                        {
                            NewBlogg.LaunchDate = DateTime.SpecifyKind(NewBlogg.LaunchDate.Date, DateTimeKind.Unspecified);
                        }
                    }
                }
            }
        }

        public Task<IActionResult> OnPostAsync(int deleteCommentId)
            => OnPostCoreAsync(deleteCommentId);

        // Skapa/ändra blogg: endast superadmin
        public async Task<IActionResult> OnPostSaveBloggAsync()
        {
            IsSuperAdmin = User.IsInRole("superadmin");
            if (!IsSuperAdmin) return Forbid();

            if (!ModelState.IsValid)
            {
                EditorAccessToken = await _bloggApi.GetEditorAccessTokenAsync();
                await OnGetCoreAsync(0, 0, false);
                return Page();
            }

            var uploadErrors = new List<string>();
            var currentBlogg = await _bloggApi.GetBloggAsync(NewBlogg.Id);

            NewBlogg.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var localDate = DateTime.SpecifyKind(NewBlogg.LaunchDate.Date, DateTimeKind.Unspecified);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, TzSe);
            NewBlogg.LaunchDate = utcDate;

            if (NewBlogg.Id == 0)
            {
                NewBlogg.Title = string.IsNullOrWhiteSpace(NewBlogg.Title) ? null : NewBlogg.Title;

                var savedBlogg = await _bloggApi.SaveBloggAsync(NewBlogg);
                if (savedBlogg == null)
                {
                    ModelState.AddModelError(string.Empty, "Kunde inte spara blogg.");
                    EditorAccessToken = await _bloggApi.GetEditorAccessTokenAsync();
                    await OnGetCoreAsync(0, 0, false);
                    return Page();
                }

                NewBlogg.Id = savedBlogg.Id;
            }
            else
            {
                if (currentBlogg == null)
                    return NotFound();

                currentBlogg.Title = string.IsNullOrWhiteSpace(NewBlogg.Title) ? null : NewBlogg.Title;
                currentBlogg.Content = NewBlogg.Content;
                currentBlogg.Author = NewBlogg.Author;
                currentBlogg.LaunchDate = NewBlogg.LaunchDate;
                currentBlogg.UserId = NewBlogg.UserId;

                await _bloggApi.UpdateBloggAsync(currentBlogg);
            }

            if (BloggImages is { Length: > 0 })
            {
                foreach (var f in BloggImages.Where(f => f != null && f.Length > 0))
                {
                    try
                    {
                        await _imageApi.UploadImageAsync(f, NewBlogg.Id);
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        uploadErrors.Add($"Kunde inte ladda upp {f.FileName}: {ex.Message}");
                    }
                }
            }

            _bloggService.InvalidateBlogListCache();

            if (uploadErrors.Count > 0)
                TempData["UploadErrors"] = string.Join("\n", uploadErrors);

            return RedirectToPage();
        }

        // Endast superadmin
        public async Task<IActionResult> OnPostSetFirstImageAsync(int imageId, int bloggId)
        {
            if (!User.IsInRole("superadmin")) return Forbid();

            var images = await _imageApi.GetImagesByBloggIdAsync(bloggId);
            var imageToSet = images.FirstOrDefault(i => i.Id == imageId);

            if (imageToSet != null)
            {
                images.Remove(imageToSet);
                images.Insert(0, imageToSet);

                await _imageApi.UpdateImageOrderAsync(bloggId, images);
                _bloggService.InvalidateBlogListCache();
            }

            return RedirectToPage(new { editId = bloggId });
        }

        // Endast superadmin
        public async Task<IActionResult> OnPostDeleteImageAsync(int imageId, int bloggId)
        {
            if (!User.IsInRole("superadmin")) return Forbid();
            await _imageApi.DeleteImageAsync(imageId);
            _bloggService.InvalidateBlogListCache();

            return RedirectToPage(new { editId = bloggId });
        }
    }
}
