namespace SarasBloggAPI.DTOs.Blogg
{
    public class BlogPostListDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<BlogPostSummaryDto> Items { get; set; } = new();
    }
}
