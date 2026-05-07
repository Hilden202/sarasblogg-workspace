using SarasBlogg.ViewModels;
using SarasBlogg.Models;
using SarasBlogg.Extensions;
using SarasBlogg.DAL;
using Microsoft.Extensions.Logging;

namespace SarasBlogg.Services
{
    public class BloggService
    {
        private readonly BloggAPIManager _bloggApi;
        private readonly CommentAPIManager _commentApi;
        private readonly BloggImageAPIManager _imageApi;
        private readonly ILogger<BloggService> _logger;

        public BloggService(
            BloggAPIManager bloggApi,
            CommentAPIManager commentApi,
            BloggImageAPIManager imageApi,
            ILogger<BloggService> logger)
        {
            _bloggApi = bloggApi;
            _commentApi = commentApi;
            _imageApi = imageApi;
            _logger = logger;
        }

        private static string MapTopRoleToCss(string? top) => top?.ToLower() switch
        {
            "superadmin" => "role-superadmin",
            "admin" => "role-admin",
            "superuser" => "role-superuser",
            "user" => "role-user",
            _ => ""
        };

        private async Task AttachImagesAsync(Blogg blogg)
            => blogg.Images = await _imageApi.GetImagesByBloggIdAsync(blogg.Id);

        public async Task<BloggViewModel> GetBloggViewModelAsync(bool isArchive, int showId = 0)
        {
            var vm = new BloggViewModel();

            // Svensk "nu"-tid för filtrering/sortering
            var nowSe = DateTime.UtcNow.ToSwedishTime();

            // Hämta alla färskt från API:t och filtrera lokalt
            var all = await GetAllBloggsAsync(includeArchived: true);

            vm.Bloggs = all
                .Where(b => (isArchive ? b.IsArchived : !b.IsArchived)
                            && !b.Hidden
                            && b.LaunchDate.ToSwedishTime() <= nowSe)
                .OrderByDescending(b => b.LaunchDate.ToSwedishTime())
                .ThenByDescending(b => b.Id)
                .ToList();

            // Säkerställ att bilder finns på de som ska visas
            foreach (var b in vm.Bloggs)
                if (b.Images == null) await AttachImagesAsync(b);

            vm.IsArchiveView = isArchive;

            if (showId != 0)
            {
                var blogg = await _bloggApi.GetBloggAsync(showId);
                if (blogg != null)
                {
                    if (blogg.Images == null) await AttachImagesAsync(blogg);

                    var existingIndex = vm.Bloggs.FindIndex(b => b.Id == showId);
                    if (existingIndex >= 0)
                    {
                        vm.Bloggs[existingIndex] = blogg;
                    }
                    else
                    {
                        vm.Bloggs.Add(blogg);
                    }
                }

                vm.Blogg = blogg;
            }

            vm.RoleCssByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (vm.Blogg is not null && vm.Blogg.Id != 0)
            {
                // Endast kommentarer för visad blogg
                var dtos = await _commentApi.GetByBloggWithRolesAsync(vm.Blogg.Id);

                vm.Comments = dtos.Select(d => new Comment
                {
                    Id = d.Id,
                    BloggId = d.BloggId,
                    Name = d.Name,
                    Content = d.Content ?? "",
                    CreatedAt = d.CreatedAt
                }).ToList();

                foreach (var d in dtos.Where(d => !string.IsNullOrWhiteSpace(d.Name)))
                {
                    var css = MapTopRoleToCss(d.TopRole);
                    if (!string.IsNullOrEmpty(css))
                        vm.RoleCssByName[d.Name] = css;
                }
            }
            else
            {
                // Lista-läget: alla kommentarer för att kunna visa räknare / rollfärg i korten
                var dtos = await _commentApi.GetAllCommentsWithRolesAsync();

                vm.Comments = dtos.Select(d => new Comment
                {
                    Id = d.Id,
                    BloggId = d.BloggId,
                    Name = d.Name,
                    Content = d.Content ?? "",
                    CreatedAt = d.CreatedAt
                }).ToList();

                foreach (var d in dtos.Where(d => !string.IsNullOrWhiteSpace(d.Name)))
                {
                    var css = MapTopRoleToCss(d.TopRole);
                    if (!string.IsNullOrEmpty(css))
                        vm.RoleCssByName[d.Name] = css;
                }
            }

            return vm;
        }

        public async Task<List<Blogg>> GetAllBloggsAsync(bool includeArchived = false)
        {
            var all = await FetchAndFilterAsync(includeArchived);

            foreach (var b in all)
                if (b.Images == null) await AttachImagesAsync(b);

            return all;
        }

        private static List<Blogg> FilterClientSide(IEnumerable<Blogg> all, bool includeArchived)
        {
            var nowSe = DateTime.UtcNow.ToSwedishTime();
            return all
                .Where(b => !b.Hidden
                            && (includeArchived || !b.IsArchived)
                            && b.LaunchDate.ToSwedishTime() <= nowSe)
                .OrderByDescending(b => b.LaunchDate.ToSwedishTime())
                .ThenByDescending(b => b.Id)
                .ToList();
        }

        private async Task<List<Blogg>> FetchAndFilterAsync(bool includeArchived)
        {
            try
            {
                var all = await _bloggApi.GetAllBloggsAsync();
                return FilterClientSide(all ?? Enumerable.Empty<Blogg>(), includeArchived);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "API-fel vid hämtning av bloggar.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "API-timeout vid hämtning av bloggar.");
            }

            return new List<Blogg>();
        }

        public async Task<string> SaveCommentAsync(Comment comment)
        {
            if (comment is null)
                return "Ogiltig kommentar.";

            comment.Content ??= string.Empty;

            return await _commentApi.SaveCommentAsync(comment) ?? string.Empty;
        }


        public Task DeleteCommentAsync(int id) => _commentApi.DeleteCommentAsync(id);
        public Task<Comment?> GetCommentAsync(int id) => _commentApi.GetCommentAsync(id);
    }
}
