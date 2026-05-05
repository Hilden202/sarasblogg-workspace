namespace SarasBloggAPI.DTOs.Comment
{
    public class CommentCreateRequest
    {
        public int BloggId { get; set; }
        public string? Name { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
