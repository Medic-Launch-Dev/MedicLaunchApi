using MedicLaunchApi.Authorization;
using MedicLaunchApi.Exceptions;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/flashcard")]
    [ApiController]
    public class FlashcardController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly FlashcardRepository flashcardRepository;

        public FlashcardController(UserManager<MedicLaunchUser> userManager, FlashcardRepository flashcardRepository)
        {
            this.userManager = userManager;
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

        [Authorize(Policy = RoleConstants.FlashcardAuthor)]
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

        [Authorize(Policy = AuthPolicies.RequireSubscriptionOrTrial)]
        [HttpGet("list")]
        public async Task<IActionResult> GetFlashcards()
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Forbid("User does not exist.");
            }

            var flashcards = await flashcardRepository.GetFlashcards(currentUser.Id);
            if (currentUser.IsOnFreeTrial)
            {
                var allowedSpecialityIds = await GetAllowedSpecialityIdsForTrialUser();
                return Ok(flashcards
                    .Where(f => allowedSpecialityIds.Contains(f.SpecialityId))
                    .ToList());
            }

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

        private async Task<List<string>> GetAllowedSpecialityIdsForTrialUser()
        {
            var specialities = await flashcardRepository.GetAllSpecialitiesAsync();
            return specialities
                .OrderBy(s => s.Name)
                .Take(4)
                .Select(s => s.Id)
                .ToList();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
