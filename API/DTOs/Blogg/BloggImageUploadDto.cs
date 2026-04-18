using System.ComponentModel.DataAnnotations;

namespace SarasBloggAPI.DTOs.Blogg
{
    public class BloggImageUploadDto
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public int BloggId { get; set; }
    }
}
