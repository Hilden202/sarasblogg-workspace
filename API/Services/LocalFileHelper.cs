using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SarasBloggAPI.Services;

namespace SarasBloggAPI.Services
{
    /// <summary>
    /// Lokal filhantering för development - sparar filer i SarasBlogg-Media mapp
    /// istället för GitHub.
    /// </summary>
    public sealed class LocalFileHelper : IFileHelper
    {
        private readonly ILogger<LocalFileHelper>? _logger;
        private readonly string _basePath;
        private readonly string _baseUrl;
        private readonly string _rootFolder = "uploads";

        public LocalFileHelper(IConfiguration cfg, IWebHostEnvironment env, ILogger<LocalFileHelper>? logger = null)
        {
            _logger = logger;

            // Basväg till SarasBlogg-Media mapp (relativ till API-projektet)
            var configuredPath = cfg["LocalStorage:BasePath"];
            if (!string.IsNullOrEmpty(configuredPath))
            {
                _basePath = Path.IsPathRooted(configuredPath)
                    ? Path.GetFullPath(configuredPath)
                    : Path.GetFullPath(configuredPath, env.ContentRootPath);
            }
            else
            {
                // Default: SarasBlogg-Media i samma katalog som API-projektet
                _basePath = Path.Combine(env.ContentRootPath, "SarasBlogg-Media");
            }

            // Base URL för att nå filerna via API
            _baseUrl = cfg["LocalStorage:BaseUrl"] ?? "https://localhost:5003/media";

            // Root folder (vanligtvis "uploads")
            _rootFolder = cfg["LocalStorage:RootFolder"] ?? "uploads";

            // Skapa basvägen om den inte finns
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            _logger?.LogInformation("LocalFileHelper initialized. BasePath: {BasePath}", _basePath);
        }

        public async Task<string?> SaveImageAsync(IFormFile file, string folderName)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";
            
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var relativePath = Path.Combine(_rootFolder, folderName ?? "misc", fileName);
            var fullPath = Path.Combine(_basePath, relativePath);

            // Skapa katalog om den inte finns
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);

                _logger?.LogInformation("Saved file: {RelativePath}", relativePath);

                // Returnera URL som kan användas för att nå filen
                return $"{_baseUrl}/{relativePath.Replace("\\", "/")}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save file: {RelativePath}", relativePath);
                throw;
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile file, int bloggId, string folderName = "blogg")
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";
            
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var relativePath = Path.Combine(_rootFolder, folderName ?? "blogg", bloggId.ToString(), fileName);
            var fullPath = Path.Combine(_basePath, relativePath);

            // Skapa katalog om den inte finns
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);

                _logger?.LogInformation("Saved file for blogg {BloggId}: {RelativePath}", bloggId, relativePath);

                // Returnera URL som kan användas för att nå filen
                return $"{_baseUrl}/{relativePath.Replace("\\", "/")}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save file for blogg {BloggId}: {RelativePath}", bloggId, relativePath);
                throw;
            }
        }

        public Task DeleteImageAsync(string imageUrl, string folder)
        {
            try
            {
                // Extrahera relativ path från URL
                var relativePath = ExtractRelativePathFromUrl(imageUrl);
                if (string.IsNullOrEmpty(relativePath))
                {
                    _logger?.LogWarning("Could not extract path from URL: {Url}", imageUrl);
                    return Task.CompletedTask;
                }

                var fullPath = Path.Combine(_basePath, relativePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger?.LogInformation("Deleted file: {RelativePath}", relativePath);
                }
                else
                {
                    _logger?.LogInformation("File not found (already deleted?): {RelativePath}", relativePath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete file: {Url}", imageUrl);
            }

            return Task.CompletedTask;
        }

        public Task DeleteBlogFolderAsync(int bloggId, string folderName = "blogg")
        {
            try
            {
                var relativePath = Path.Combine(_rootFolder, folderName ?? "blogg", bloggId.ToString());
                var fullPath = Path.Combine(_basePath, relativePath);

                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, recursive: true);
                    _logger?.LogInformation("Deleted folder for blogg {BloggId}: {RelativePath}", bloggId, relativePath);
                }
                else
                {
                    _logger?.LogInformation("Folder not found for blogg {BloggId}: {RelativePath}", bloggId, relativePath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete folder for blogg {BloggId}", bloggId);
            }

            return Task.CompletedTask;
        }

        private string? ExtractRelativePathFromUrl(string url)
        {
            // URL format: https://localhost:5003/media/uploads/...
            // Vi vill ha: uploads/...
            
            if (url.Contains("/media/"))
            {
                var parts = url.Split("/media/", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    return parts[1].Replace("/", "\\");
                }
            }

            // Om det redan är en relativ path
            if (!url.StartsWith("http"))
            {
                return url.Replace("/", "\\");
            }

            return null;
        }
    }
}
