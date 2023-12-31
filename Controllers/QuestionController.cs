using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MedicLaunchApi.Controllers
{
    //[Authorize]
    [Route("api/questions")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> logger;
        private readonly QuestionRepository questionRepository;

        public QuestionController(ILogger<QuestionController> logger, QuestionRepository questionRepository)
        {
            this.logger = logger;
            this.questionRepository = questionRepository;
        }

        // POST api/questions/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] QuestionViewModel model)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                this.logger.LogError("User is not authenticated");
                return Unauthorized();
            }

            await CreateQuestion(model, currentUserId);

            return Ok();
        }

        // POST api/questions/update/{questionId}
        [HttpPost("update/{questionId}")]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionViewModel model, string questionId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                this.logger.LogError("User is not authenticated");
                return Unauthorized();
            }

            // If specialty is changed, we should delete question from the previous speciality and create a new question under the new speciality
            if(model.PreviousSpecialityId != model.SpecialityId)
            {
                await this.questionRepository.DeleteQuestionAsync(model.PreviousSpecialityId, questionId, CancellationToken.None);
                await CreateQuestion(model, currentUserId, questionId);
                return Ok();
            }

            // otherwise, update the existing question and save it
            var question = new Question
            {
                Id = questionId,
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                LabValues = model.LabValues,
                Options = model.Options,
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                References = model.References,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserId
            };

            await this.questionRepository.UpdateQuestionAsync(question, CancellationToken.None);
            return Ok();
        }

        // GET api/questions/speciality/{specialityId}
        [HttpGet("speciality/{specialityId}")]
        public async Task<IActionResult> GetQuestions(string specialityId)
        {
            // TODO: convert to question view model - use auto mapper
            var questions = await this.questionRepository.GetQuestionsAsync(specialityId, CancellationToken.None);
            return Ok(questions);
        }

        private async Task CreateQuestion(QuestionViewModel model, string currentUserId, string? questionId = null)
        {
            // TODO: add question code
            var question = new Question
            {
                Id = questionId ?? Guid.NewGuid().ToString(),
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                LabValues = model.LabValues,
                Options = model.Options,
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                References = model.References,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserId,
                AuthorUserId = currentUserId
            };

            await this.questionRepository.CreateQuestionAsync(question, CancellationToken.None);
        }
    }
}
