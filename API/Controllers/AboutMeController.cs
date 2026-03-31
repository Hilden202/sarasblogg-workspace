using Microsoft.AspNetCore.Mvc;
using SarasBloggAPI.DAL;
using SarasBloggAPI.Models;
using SarasBloggAPI.Services;
using SarasBloggAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace SarasBloggAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOrSuperadmin")] // standardkrav
    public class AboutMeController : ControllerBase
    {
        private readonly AboutMeManager _manager;
        private readonly IAboutMeImageService _imgSvc;

        public AboutMeController(AboutMeManager manager, IAboutMeImageService imgSvc)
        {
            _manager = manager;
            _imgSvc = imgSvc;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<AboutMeDto?>> GetAboutMe()
        {
            var aboutMe = await _manager.GetAsync();
            if (aboutMe == null) return NotFound();
            return Ok(ToDto(aboutMe));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAboutMe([FromBody] AboutMeDto dto)
        {
            var aboutMe = ToEntity(dto);
            var created = await _manager.CreateAsync(aboutMe);
            return CreatedAtAction(nameof(GetAboutMe), new { id = created.Id }, ToDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAboutMe(int id, [FromBody] AboutMeDto dto)
        {
            if (dto == null)
                return BadRequest("Ingen data skickades.");

            var aboutMe = ToEntity(dto);

            // säkerställ att route-id gäller
            aboutMe.Id = id;

            var updated = await _manager.UpdateAsync(aboutMe);
            if (!updated)
                return NotFound($"Ingen AboutMe med Id={id} hittades.");

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAboutMe(int id)
        {
            var success = await _manager.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

        // ---- Bildendpoints ----

        [AllowAnonymous]
        [HttpGet("image")]
        public async Task<ActionResult<AboutMeImageDto>> GetImage()
        {
            var url = await _imgSvc.GetCurrentUrlAsync();
            return Ok(new AboutMeImageDto { ImageUrl = url });
        }

        [HttpPut("image")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<AboutMeImageDto>> PutImage([FromForm] AboutMeImageUploadDto dto)
        {
            if (dto.File is null || dto.File.Length == 0)
                return BadRequest("Ingen bild bifogad.");

            var url = await _imgSvc.UploadOrReplaceAsync(dto.File);
            return Ok(new AboutMeImageDto { ImageUrl = url });
        }

        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage()
        {
            await _imgSvc.DeleteAsync();
            return NoContent();
        }

        private static AboutMeDto ToDto(AboutMe aboutMe) => new()
        {
            Id = aboutMe.Id,
            Title = aboutMe.Title,
            Content = aboutMe.Content,
            Image = aboutMe.Image,
            Name = aboutMe.Name,
            City = aboutMe.City,
            Age = aboutMe.Age,
            Family = aboutMe.Family,
            UserId = aboutMe.UserId
        };

        private static AboutMe ToEntity(AboutMeDto dto) => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.Content,
            Image = dto.Image,
            Name = dto.Name,
            City = dto.City,
            Age = dto.Age,
            Family = dto.Family,
            UserId = dto.UserId
        };
    }
}
