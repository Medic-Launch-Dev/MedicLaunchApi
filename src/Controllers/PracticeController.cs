using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MedicLaunchApi.Repository;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/practice")]
    [ApiController]
    public class PracticeController : ControllerBase
    {
        private readonly QuestionRepository questionRepository;
        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public PracticeController(QuestionRepository questionRepository)
        {
            this.questionRepository = questionRepository;
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(QuestionAttemptRequest questionAttempt)
        {
            await this.questionRepository.AttemptQuestionAsync(questionAttempt, CurrentUserId);
            return Ok();
        }

        [HttpPost("flagquestion/{questionId}")]
        public async Task<IActionResult> FlagQuestion(string questionId)
        {
            await this.questionRepository.AddFlaggedQuestionAsync(questionId, CurrentUserId);
            return Ok();
        }

        [HttpGet("practicestats")]
        public async Task<PracticeStats> GetPracticeStats()
        {
            return await this.questionRepository.GetPracticeStatsAsync(CurrentUserId);
        }

        [HttpPost("filter")]
        public async Task<IEnumerable<QuestionViewModel>> FilterQuestions(QuestionsFilterRequest filterRequest)
        {
            return await this.questionRepository.FilterQuestionsAsync(filterRequest, CurrentUserId);
        }

        [HttpPost("familiaritycounts")]
        public Task<QuestionFamiliarityCounts> GetQuestionFamiliarityCounts([FromBody] FamiliarityCountsRequest request)
        {
            return this.questionRepository.GetQuestionFamiliarityCountsAsync(CurrentUserId, request);
        }

        [HttpPost("unflagquestion/{questionId}")]
        public async Task<IActionResult> UnflagQuestion(string questionId)
        {
            await this.questionRepository.RemoveFlaggedQuestionAsync(questionId, CurrentUserId);
            return Ok();
        }

        // Add endpoint to reset attempted and flagged questions for user
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
    }
}
