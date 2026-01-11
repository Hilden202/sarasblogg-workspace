using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SarasBloggAPI.Data;
using SarasBloggAPI.Models;
using SarasBloggAPI.Services;

namespace SarasBloggAPI.DAL
{
    public class BloggManager
    {
        private readonly MyDbContext _context;
        private readonly BloggImageManager _imageManager;
        private readonly IFileHelper _fileHelper;
        private readonly ILogger<BloggManager> _logger;

        public BloggManager(MyDbContext context, BloggImageManager imageManager, IFileHelper fileHelper, ILogger<BloggManager> logger)
        {
            _context = context;
            _imageManager = imageManager;
            _fileHelper = fileHelper;
            _logger = logger;
        }

        public async Task<List<Blogg>> GetAllAsync()
        {
            return await _context.Bloggs.OrderByDescending(b => b.LaunchDate).ToListAsync();
        }

        public async Task<Blogg?> GetByIdAsync(int id)
        {
            return await _context.Bloggs.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Blogg> CreateAsync(Blogg blogg)
        {
            _context.Bloggs.Add(blogg);
            await _context.SaveChangesAsync();
            return blogg;
        }

        public async Task<bool> UpdateAsync(Blogg blogg)
        {
            var existing = await _context.Bloggs
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == blogg.Id);

            if (existing == null)
                return false;

            await DeleteRemovedInlineImagesAsync(existing, blogg);

            _context.Bloggs.Update(blogg);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // (1) Försök ta bort GitHub-mappen för bloggen
            try
            {
                await _fileHelper.DeleteBlogFolderAsync(id, "blogg");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunde inte radera GitHub-mapp för blogg {Id}", id);
                // vi fortsätter ändå så DB inte fastnar
            }

            // (2) Ta bort alla bildposter i DB
            await _imageManager.DeleteImagesByBloggIdAsync(id);

            // (3) Ta bort själva bloggen
            var blogg = await _context.Bloggs.FindAsync(id);
            if (blogg == null) return false;

            _context.Bloggs.Remove(blogg);
            return await _context.SaveChangesAsync() > 0;
        }
        
        private async Task DeleteRemovedInlineImagesAsync(Blogg existing, Blogg updated)
        {
            var oldUrls = ExtractImageUrls(existing.Content);
            var newUrls = ExtractImageUrls(updated.Content);

            if (oldUrls.Count == 0)
                return;

            var removedUrls = oldUrls.Except(newUrls, StringComparer.OrdinalIgnoreCase);

            foreach (var url in removedUrls)
            {
                if (!ShouldDeleteImageUrl(url))
                    continue;

                try
                {
                    await _fileHelper.DeleteImageAsync(url, "blogg");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kunde inte radera borttagen inline-bild: {Url}", url);
                }
            }
        }

        private static HashSet<string> ExtractImageUrls(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var matches = Regex.Matches(html, "<img[^>]*?\\s+src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in matches)
            {
                var src = match.Groups["src"].Value?.Trim();
                if (string.IsNullOrWhiteSpace(src))
                    continue;

                urls.Add(src);
            }

            return urls;
        }

        private static bool ShouldDeleteImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return false;

            return url.Contains("/uploads/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
