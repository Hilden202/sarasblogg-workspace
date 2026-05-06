using Microsoft.AspNetCore.Mvc;
using SarasBloggAPI.DAL;
using Microsoft.AspNetCore.Authorization;
using SarasBloggAPI.Services.Blogg;
using Microsoft.EntityFrameworkCore;
using SarasBloggAPI.Data;
using SarasBloggAPI.DTOs.Blogg;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BloggModel = SarasBloggAPI.Models.Blogg;


namespace SarasBloggAPI.Controllers.Blogg
{
    [ApiController]
    [Route("api/[controller]")] // => api/blogg
    public class BloggController : ControllerBase
    {
        private readonly BloggManager _BloggManager;
        private readonly BlogPostService _blogPostService;
        private readonly MyDbContext _db;
        private readonly ILogger<BloggController> _logger;

        public BloggController(BloggManager bloggManager, BlogPostService blogPostService, MyDbContext db, ILogger<BloggController> logger)
        {
            _BloggManager = bloggManager;
            _blogPostService = blogPostService;
            _db = db;
            _logger = logger;
        }

        // GET: api/blogg
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<BloggModel>>> GetAll()
        {
            var bloggs = await _BloggManager.GetAllAsync();
            return Ok(bloggs);
        }

        // GET: api/blogg/public
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<ActionResult<BlogPostListDto>> GetPublic([FromQuery] bool archive = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = PublicBloggsQuery(DateTime.UtcNow, archive);
            var totalItems = await query.CountAsync();
            var bloggs = await query
                .OrderByDescending(b => b.LaunchDate)
                .ThenByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new BlogPostListDto
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = bloggs.Select(ToSummaryDto).ToList()
            });
        }

        // GET: api/blogg/public/5 or api/blogg/public/5-title
        [AllowAnonymous]
        [HttpGet("public/{idOrSlug}")]
        public async Task<ActionResult<BlogPostDetailDto>> GetPublicByIdOrSlug(string idOrSlug, [FromQuery] bool archive = false)
        {
            var query = PublicBloggsQuery(DateTime.UtcNow, archive);
            BloggModel? blogg = null;

            if (TryGetId(idOrSlug, out var id))
            {
                blogg = await query.FirstOrDefaultAsync(b => b.Id == id);
            }
            else
            {
                var requestedSlug = CreateTitleSlug(idOrSlug);
                var candidates = await query
                    .OrderByDescending(b => b.LaunchDate)
                    .ThenByDescending(b => b.Id)
                    .ToListAsync();

                blogg = candidates.FirstOrDefault(b => CreateTitleSlug(b.Title) == requestedSlug);
            }

            if (blogg == null)
                return NotFound();

            return Ok(ToDetailDto(blogg));
        }

        // GET: api/blogg/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<BloggModel>> Get(int id)
        {
            var blogg = await _BloggManager.GetByIdAsync(id);
            if (blogg == null)
                return NotFound();

            return Ok(blogg);
        }

        // POST: api/blogg
        [Authorize(Policy = "AdminOrSuperadmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogPostWriteRequest request)
        {
            _logger.LogInformation(
                "Create blog - Source={Source}",
                request.GetLegacyId().HasValue ? "Legacy" : "DTO"
            );
            var created = await _blogPostService.CreateAsync(request, User);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT: api/blogg/5
        [Authorize(Policy = "AdminOrSuperadmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogPostWriteRequest request)
        {
            _logger.LogInformation(
                "Update blog - RouteId={RouteId}, Source={Source}",
                id,
                request.GetLegacyId().HasValue ? "Legacy" : "DTO"
            );
            var legacyId = request.GetLegacyId();
            if (legacyId.HasValue && id != legacyId.Value)
                return BadRequest();

            var updated = await _blogPostService.UpdateAsync(id, request, User);
            return updated != null ? NoContent() : NotFound();
        }

        [Authorize(Policy = "AdminOrSuperadmin")]
        [HttpPatch("{id}/hidden")]
        public async Task<IActionResult> ToggleHidden(int id)
        {
            var b = await _BloggManager.GetByIdAsync(id);
            if (b == null) return NotFound();
            b.Hidden = !b.Hidden;
            var ok = await _BloggManager.UpdateAsync(b);
            return ok ? Ok(new { b.Hidden }) : StatusCode(500, "Update failed");
        }

        [Authorize(Policy = "AdminOrSuperadmin")]
        [HttpPatch("{id}/archived")]
        public async Task<IActionResult> ToggleArchived(int id)
        {
            var b = await _BloggManager.GetByIdAsync(id);
            if (b == null) return NotFound();
            b.IsArchived = !b.IsArchived;
            var ok = await _BloggManager.UpdateAsync(b);
            return ok ? Ok(new { b.IsArchived }) : StatusCode(500, "Update failed");
        }


        // DELETE: api/blogg/5
        [Authorize(Policy = "AdminOrSuperadmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _BloggManager.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        private IQueryable<BloggModel> PublicBloggsQuery(DateTime nowUtc, bool archive)
        {
            return _db.Bloggs
                .AsNoTracking()
                .Include(b => b.Images)
                .Where(b => !b.Hidden && b.LaunchDate <= nowUtc && b.IsArchived == archive);
        }

        private static BlogPostSummaryDto ToSummaryDto(BloggModel blogg)
        {
            return new BlogPostSummaryDto
            {
                Id = blogg.Id,
                Slug = CreateSlug(blogg),
                Title = blogg.Title ?? string.Empty,
                Author = blogg.Author,
                Excerpt = CreateExcerpt(blogg.Content),
                PublishedAtUtc = blogg.LaunchDate,
                IsArchived = blogg.IsArchived,
                ViewCount = blogg.ViewCount,
                CoverImage = GetCoverImage(blogg)
            };
        }

        private static BlogPostDetailDto ToDetailDto(BloggModel blogg)
        {
            var images = GetOrderedImages(blogg).ToList();

            return new BlogPostDetailDto
            {
                Id = blogg.Id,
                Slug = CreateSlug(blogg),
                Title = blogg.Title ?? string.Empty,
                Content = blogg.Content,
                Author = blogg.Author,
                PublishedAtUtc = blogg.LaunchDate,
                IsArchived = blogg.IsArchived,
                ViewCount = blogg.ViewCount,
                CoverImage = images.FirstOrDefault(),
                Images = images
            };
        }

        private static BloggImageDto? GetCoverImage(BloggModel blogg)
        {
            return GetOrderedImages(blogg).FirstOrDefault();
        }

        private static IEnumerable<BloggImageDto> GetOrderedImages(BloggModel blogg)
        {
            return (blogg.Images ?? Enumerable.Empty<SarasBloggAPI.Models.BloggImage>())
                .OrderBy(i => i.Order)
                .ThenBy(i => i.Id)
                .Select(i => new BloggImageDto
                {
                    Id = i.Id,
                    BloggId = i.BloggId,
                    FilePath = i.FilePath,
                    Order = i.Order
                });
        }

        private static bool TryGetId(string idOrSlug, out int id)
        {
            if (int.TryParse(idOrSlug, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                return true;

            var dashIndex = idOrSlug.IndexOf('-');
            return dashIndex > 0
                && int.TryParse(idOrSlug[..dashIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        private static string CreateSlug(BloggModel blogg)
        {
            if (string.IsNullOrWhiteSpace(blogg.Title))
                return blogg.Id.ToString(CultureInfo.InvariantCulture);

            return $"{blogg.Id}-{CreateTitleSlug(blogg.Title)}";
        }

        private static string CreateTitleSlug(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            var previousWasDash = false;

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                var lower = char.ToLowerInvariant(c);
                if (char.IsLetterOrDigit(lower))
                {
                    builder.Append(lower);
                    previousWasDash = false;
                    continue;
                }

                if (!previousWasDash && builder.Length > 0)
                {
                    builder.Append('-');
                    previousWasDash = true;
                }
            }

            return builder.ToString().Trim('-');
        }

        private static string CreateExcerpt(string? html, int maxLength = 180)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = Regex.Replace(html, "<.*?>", " ");
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, "\\s+", " ").Trim();

            if (text.Length <= maxLength)
                return text;

            return text[..maxLength].TrimEnd() + "...";
        }
    }
}
