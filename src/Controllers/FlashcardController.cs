using MedicLaunchApi.Authorization;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/flashcard")]
    [ApiController]
    public class FlashcardController : ControllerBase
    {
        private readonly FlashcardRepository flashcardRepository;

        public FlashcardController(FlashcardRepository flashcardRepository)
        {
            this.flashcardRepository = flashcardRepository;
        }

        [Authorize(Policy = RoleConstants.FlashcardAuthor)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateFlashcard([FromBody] CreateFlashcardRequest request)
        {
            await flashcardRepository.CreateFlashcardAsync(request, GetCurrentUserId());

            return Ok();
        }

        [Authorize(Policy = RoleConstants.FlashcardAuthor)]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateFlashcard([FromBody] UpdateFlashcardRequest request)
        {
            try
            {
                bool isAdmin = User.IsInRole(RoleConstants.Admin);
                var flashcard = await flashcardRepository.UpdateFlashcardAsync(request, GetCurrentUserId(), isAdmin);

                if (flashcard == null)
                {
                    return NotFound();
                }

                return Ok(flashcard);
            }
            catch (AccessDeniedException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlashcard(string id)
        {
            var flashcard = await flashcardRepository.GetFlashcard(id);

            if (flashcard == null)
            {
                return NotFound();
            }

            return Ok(flashcard);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetFlashcards()
        {
            var flashcards = await flashcardRepository.GetFlashcards();
            return Ok(flashcards);
        }

        [Authorize(Policy = RoleConstants.FlashcardAuthor)]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFlashcard(string id)
        {
            await flashcardRepository.DeleteFlashcardAsync(id);

            return Ok();
        }

        [Authorize(Policy = RoleConstants.FlashcardAuthor)]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var imageUrl = await flashcardRepository.UploadImageAsync(file);

            return Ok(imageUrl);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
