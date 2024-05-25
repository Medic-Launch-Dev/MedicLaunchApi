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

        [HttpPost("create")]
        public async Task<IActionResult> CreateFlashcard([FromBody] CreateFlashcardRequest request)
        {
            await flashcardRepository.CreateFlashcardAsync(request, GetCurrentUserId());

            return Ok();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateFlashcard([FromBody] UpdateFlashcardRequest request)
        {
            var flashcard = await flashcardRepository.UpdateFlashcardAsync(request, GetCurrentUserId());

            if (flashcard == null)
            {
                return NotFound();
            }

            return Ok(flashcard);
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFlashcard(string id)
        {
            await flashcardRepository.DeleteFlashcardAsync(id);

            return Ok();
        }

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
