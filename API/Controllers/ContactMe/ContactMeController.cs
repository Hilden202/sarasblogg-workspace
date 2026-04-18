using Microsoft.AspNetCore.Mvc;
using SarasBloggAPI.DAL;
using Microsoft.AspNetCore.Authorization;
using ContactMeModel = SarasBloggAPI.Models.ContactMe;

namespace SarasBloggAPI.Controllers.ContactMe
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOrSuperadmin")]
    public class ContactMeController : ControllerBase
    {
        private readonly ContactMeManager _manager;

        public ContactMeController(ContactMeManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public async Task<ActionResult<List<ContactMeModel>>> GetAll()
        {
            var contacts = await _manager.GetAllAsync();
            return Ok(contacts);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ContactMeModel>> Create(ContactMeModel contact)
        {
            var created = await _manager.CreateAsync(contact);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _manager.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
