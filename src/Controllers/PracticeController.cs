using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MedicLaunchApi.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MedicLaunchApi.Authorization;

namespace MedicLaunchApi.Controllers
{
    [Route("api/practice")]
    [ApiController]
    [Authorize]
    public class PracticeController : ControllerBase
    {
        private readonly QuestionRepository questionRepository;
        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        private readonly UserManager<MedicLaunchUser> userManager;

        public PracticeController(QuestionRepository questionRepository, UserManager<MedicLaunchUser> userManager)
        {
            this.questionRepository = questionRepository;
            this.userManager = userManager;
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(QuestionAttemptRequest questionAttempt)
        {
            var hasActiveSubscription = await HasActiveSubscription();
            if (!hasActiveSubscription)
            {
                return Forbid();
            }

            await this.questionRepository.AttemptQuestionAsync(questionAttempt, CurrentUserId);
            return Ok();
        }

        [HttpPost("flagquestion/{questionId}")]
        public async Task<IActionResult> FlagQuestion(string questionId)
        {
            var hasActiveSubscription = await HasActiveSubscription();
            if (!hasActiveSubscription)
            {
                return Forbid();
            }

            await this.questionRepository.AddFlaggedQuestionAsync(questionId, CurrentUserId);
            return Ok();
        }

        [HttpGet("practicestats")]
        public async Task<PracticeStats> GetPracticeStats()
        {
            return await this.questionRepository.GetPracticeStatsAsync(CurrentUserId);
        }

        [HttpPost("filter")]
        public async Task<IActionResult> FilterQuestions(QuestionsFilterRequest filterRequest)
        {
            var hasActiveSubscription = await HasActiveSubscription();
            if (!hasActiveSubscription)
            {
                return Forbid();
            }

            return Ok(await this.questionRepository.FilterQuestionsAsync(filterRequest, CurrentUserId));
        }

        [HttpPost("familiaritycounts")]
        public async Task<QuestionFamiliarityCounts> GetQuestionFamiliarityCounts([FromBody] FamiliarityCountsRequest request)
        {
            return await this.questionRepository.GetQuestionFamiliarityCountsAsync(CurrentUserId, request);
        }

        [HttpPost("unflagquestion/{questionId}")]
        public async Task<IActionResult> UnflagQuestion(string questionId)
        {
            await this.questionRepository.RemoveFlaggedQuestionAsync(questionId, CurrentUserId);
            return Ok();
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetQuestions()
        {
            await this.questionRepository.ResetUserPracticeAsync(CurrentUserId);
            return Ok();
        }

        [HttpGet("specialityanalytics")]
        public async Task<IActionResult> GetSpecialityAnalytics()
        {
            var result = await this.questionRepository.GetSpecialityAnalytics(CurrentUserId);
            return Ok(result );
        }

        private async Task<bool> HasActiveSubscription()
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return false;
            }

            // If user is Admin or QuestionAuthor, they have access to all questions
            if (User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.QuestionAuthor))
            {
                return true;
            }

            return user.SubscriptionExpiryDate.HasValue && user.SubscriptionExpiryDate.Value > DateTime.UtcNow;
        }
    }
}
