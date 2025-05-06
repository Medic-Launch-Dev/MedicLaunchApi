using MedicLaunchApi.Authorization;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/note")]
    [ApiController]
    [Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
    public class NotesController : ControllerBase
    {
        private readonly UserDataRepository userDataRepository;

        public NotesController(UserDataRepository userDataRepository)
        {
            this.userDataRepository = userDataRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNoteAsync([FromBody] CreateNoteRequest request)
        {
            if (request.SpecialityId == null && request.QuestionId == null && request.FlashcardId == null)
            {
                return BadRequest("A note should be associated with a speciality, question, or a flashcard");
            }

            await userDataRepository.CreateNoteAsync(request, GetCurrentUserId());

            return Ok();
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetNotesAsync()
        {
            var notes = await userDataRepository.GetNotesAsync(GetCurrentUserId());

            return Ok(notes);
        }

        [HttpDelete("delete/{noteId}")]
        public async Task<IActionResult> DeleteNoteAsync(string noteId)
        {
            await userDataRepository.DeleteNoteAsync(noteId, GetCurrentUserId());

            return Ok();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateNoteAsync([FromBody] UpdateNoteRequest request)
        {
            if (request.SpecialityId == null && request.QuestionId == null && request.FlashcardId == null)
            {
                return BadRequest("A note should be associated with a speciality, question, or a flashcard");
            }

            await userDataRepository.UpdateNoteAsync(request, GetCurrentUserId());

            return Ok();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
