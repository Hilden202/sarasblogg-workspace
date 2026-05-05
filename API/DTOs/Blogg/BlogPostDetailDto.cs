namespace SarasBloggAPI.DTOs.Blogg
{
    public class BlogPostDetailDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishedAtUtc { get; set; }
        public bool IsArchived { get; set; }
        public int ViewCount { get; set; }
        public BloggImageDto? CoverImage { get; set; }
        public List<BloggImageDto> Images { get; set; } = new();
    }
}
