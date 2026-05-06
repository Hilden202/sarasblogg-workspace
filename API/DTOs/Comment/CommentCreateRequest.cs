using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SarasBloggAPI.DTOs.Comment
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class CommentCreateRequest
    {
        [Range(1, int.MaxValue)]
        public int BloggId { get; set; }

        public string? Name { get; set; }

        [Required]
        public string? Content { get; set; }
    }
}
