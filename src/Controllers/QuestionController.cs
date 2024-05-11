using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
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

        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionViewModel model)
        {
            string currentUserId = GetCurrentUserId();
            await this.questionRepository.CreateQuestionAsync(model, currentUserId);
            return Created();
        }

        [HttpPost("update/{questionId}")]
        public async Task<IActionResult> Update([FromBody] QuestionViewModel model, string questionId)
        {
            await this.questionRepository.UpdateQuestionAsync(model, questionId, GetCurrentUserId());
            return Ok();
        }

        [HttpGet("speciality/{specialityId}")]
        public async Task<IEnumerable<QuestionViewModel>> GetQuestions(string specialityId)
        {
            var questions = await this.questionRepository.GetQuestionsInSpecialityAsync(specialityId);
            return CreateQuestionViewModel(questions);
        }


        [HttpDelete("delete/{specialityId}/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(string specialityId, string questionId)
        {
            await this.questionRepository.DeleteQuestionAsync(questionId);
            return Ok();
        }

        [HttpPost("speciality/create")]
        public async Task<IActionResult> CreateSpeciality([FromBody] SpecialityViewModel model)
        {
            var speciality = new Data.Speciality
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name
            };
            await this.questionRepository.AddSpecialityAsync(speciality);
            return Ok(speciality);
        }

        [HttpPost("speciality/bulk-create")]
        public async Task<IActionResult> CreateSpecialities([FromBody] IEnumerable<SpecialityViewModel> specialities)
        {
            foreach (var speciality in specialities)
            {
                await CreateSpeciality(speciality);
            }

            return Ok();
        }

        [HttpGet("specialities")]
        public async Task<IEnumerable<SpecialityViewModel>> GetSpecialities()
        {
            var specialties = await this.questionRepository.GetSpecialitiesAsync();
            return specialties.Select(s => new SpecialityViewModel
            {
                Id = s.Id,
                Name = s.Name
            });
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(QuestionAttemptRequest questionAttempt)
        {
            await this.questionRepository.AttemptQuestionAsync(questionAttempt, GetCurrentUserId());
            return Ok();
        }

        [HttpPost("flagquestion/{questionId}")]
        public async Task<IActionResult> FlagQuestion(string questionId)
        {
            await this.questionRepository.AddFlaggedQuestionAsync(questionId, GetCurrentUserId());
            return Ok();
        }

        [HttpGet("practicestats")]
        public async Task<PracticeStats> GetPracticeStats()
        {
            return await this.questionRepository.GetPracticeStatsAsync(GetCurrentUserId());
        }

        [HttpPost("filter")]
        public async Task<IEnumerable<QuestionViewModel>> FilterQuestions(QuestionsFilterRequest filterRequest)
        {
            var questions = await this.questionRepository.FilterQuestionsAsync(filterRequest, GetCurrentUserId());
            return CreateQuestionViewModel(questions);
        }

        //[HttpPost("familiaritycounts")]
        //public Task<QuestionFamiliarityCounts> GetQuestionFamiliarityCounts([FromBody] FamiliarityCountsRequest request)
        //{
        //    return this.practiceService.GetCategoryCounts(GetCurrentUserId(), request);
        //}

        private IEnumerable<QuestionViewModel> CreateQuestionViewModel(IEnumerable<MedicLaunchApi.Data.Question> questions)
        {
            return questions.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                SpecialityId = q.SpecialityId,
                QuestionType = q.QuestionType.ToString(),
                QuestionText = q.QuestionText,
                Options = q.Options.Select(m => new Option()
                {
                    Letter = m.Letter,
                    Text = m.Text
                }),
                CorrectAnswerLetter = q.CorrectAnswerLetter,
                Explanation = q.Explanation,
                ClinicalTips = q.ClinicalTips,
                LearningPoints = q.LearningPoints,
                QuestionCode = q.Code,
                SpecialityName = q.Speciality.Name
            }).ToList();
        }
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
