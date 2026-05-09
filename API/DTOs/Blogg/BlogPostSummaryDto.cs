namespace SarasBloggAPI.DTOs.Blogg
{
    public class BlogPostSummaryDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool ShowTitle { get; set; }
        public bool IsTitleGenerated { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public int ReadingTimeMinutes { get; set; }
        public DateTime PublishedAtUtc { get; set; }
        public bool IsArchived { get; set; }
        public int ViewCount { get; set; }
        public BloggImageDto? CoverImage { get; set; }
    }
}
