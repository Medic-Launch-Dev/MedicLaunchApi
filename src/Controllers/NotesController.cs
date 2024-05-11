using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly UserDataRepository userDataRepository;

        public NotesController(UserDataRepository userDataRepository)
        {
            this.userDataRepository = userDataRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNoteAsync([FromBody] CreateNoteRequest request)
        {
            await userDataRepository.CreateNoteAsync(request, GetCurrentUserId());

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotesAsync()
        {
            var notes = await userDataRepository.GetNotesAsync(GetCurrentUserId());

            return Ok(notes);
        }

        [HttpDelete("{noteId}")]
        public async Task<IActionResult> DeleteNoteAsync(string noteId)
        {
            await userDataRepository.DeleteNoteAsync(noteId, GetCurrentUserId());

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNoteAsync([FromBody] UpdateNoteRequest request)
        {
            await userDataRepository.UpdateNoteAsync(request, GetCurrentUserId());

            return Ok();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
