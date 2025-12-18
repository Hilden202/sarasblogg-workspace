using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SarasBloggAPI.Controllers
{
    /// <summary>
    /// Serverar media-filer från lokal SarasBlogg-Media mapp i Development-miljö.
    /// I Test/Prod serveras filer direkt från GitHub via raw.githubusercontent.com.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly string _basePath;
        private readonly ILogger<MediaController> _logger;
        private readonly IWebHostEnvironment _env;

        public MediaController(IConfiguration config, ILogger<MediaController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;

            // Basväg till SarasBlogg-Media mapp
            var configuredPath = config["LocalStorage:BasePath"];
            if (!string.IsNullOrEmpty(configuredPath))
            {
                _basePath = Path.GetFullPath(configuredPath);
            }
            else
            {
                var apiDir = Directory.GetCurrentDirectory();
                _basePath = Path.Combine(apiDir, "..", "SarasBlogg-Media");
            }
        }

        /// <summary>
        /// Serverar filer från SarasBlogg-Media mappen.
        /// Exempel: GET /media/uploads/blogg/1/abc123.jpg
        /// </summary>
        [HttpGet("{**path}")]
        public IActionResult GetMedia(string path)
        {
            // Endast tillgängligt i Development
            if (!_env.IsDevelopment())
            {
                return NotFound("Media endpoint is only available in Development mode.");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path is required.");
            }

            // Säkerhet: förhindra directory traversal
            var fullPath = Path.GetFullPath(Path.Combine(_basePath, path));
            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempted directory traversal: {Path}", path);
                return BadRequest("Invalid path.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogInformation("File not found: {FullPath}", fullPath);
                return NotFound();
            }

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return PhysicalFile(fullPath, contentType);
        }
    }
}
