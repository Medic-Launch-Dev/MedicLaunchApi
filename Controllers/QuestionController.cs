using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using ApplicationDbContext = MedicLaunchApi.Data.ApplicationDbContext;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/questions")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> logger;
        private readonly QuestionRepositoryLegacy questionRepositoryLegacy;
        private readonly PracticeService practiceService;
        private readonly QuestionRepository questionRepository;
        private readonly ApplicationDbContext dbContext;

        public QuestionController(ILogger<QuestionController> logger, QuestionRepositoryLegacy questionRepositoryLegacy, PracticeService practiceService, QuestionRepository questionRepository, ApplicationDbContext dbContext)
        {
            this.logger = logger;
            this.questionRepositoryLegacy = questionRepositoryLegacy;
            this.practiceService = practiceService;
            this.questionRepository = questionRepository;
            this.dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] QuestionViewModel model)
        {
            string currentUserId = GetCurrentUserId();
            string questionCode = await this.questionRepository.GenerateQuestionCode(model.SpecialityId);
            await CreateQuestion(model, questionCode, currentUserId);

            return Ok();
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
            var speciality = new Speciality
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name
            };
            await this.questionRepositoryLegacy.AddSpeciality(speciality, CancellationToken.None);
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
            return await this.questionRepositoryLegacy.GetSpecialities(CancellationToken.None);
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(QuestionAttemptRequest questionAttempt)
        {
            await this.questionRepository.AttemptQuestionAsync(questionAttempt, GetCurrentUserId());
            return Ok();

            //var attempt = new QuestionAttempt
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    QuestionId = questionAttempt.QuestionId,
            //    ChosenAnswer = questionAttempt.ChosenAnswer,
            //    CorrectAnswer = questionAttempt.CorrectAnswer,
            //    IsCorrect = questionAttempt.IsCorrect,
            //    CreatedAt = DateTime.UtcNow,
            //    UpdatedAt = DateTime.UtcNow
            //};

            //await this.questionRepositoryLegacy.AddQuestionAttempt(attempt, GetCurrentUserId());

            //var practiceStats = await this.questionRepositoryLegacy.GetPracticeStatsAsync(GetCurrentUserId());
            //if (practiceStats != null)
            //{
            //    if (attempt.IsCorrect)
            //    {
            //        practiceStats.TotalCorrect++;
            //    }
            //    else
            //    {
            //        practiceStats.TotalIncorrect++;
            //    }

            //    practiceStats.UpdatedAt = DateTime.UtcNow;
            //}
            //else
            //{
            //    practiceStats = new PracticeStats
            //    {
            //        Id = Guid.NewGuid().ToString(),
            //        TotalCorrect = attempt.IsCorrect ? 1 : 0,
            //        TotalIncorrect = attempt.IsCorrect ? 0 : 1,
            //        CreatedAt = DateTime.UtcNow,
            //        UpdatedAt = DateTime.UtcNow
            //    };
            //}

            //await this.questionRepositoryLegacy.CreateOrUpdatePracticeStats(practiceStats, GetCurrentUserId());

            //return Ok();
        }

        [HttpPost("flagquestion")]
        public async Task<IActionResult> FlagQuestion(string questionId)
        {
            var questionFlagged = new FlaggedQuestion
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = questionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.questionRepositoryLegacy.AddQuestionFlagged(questionFlagged, GetCurrentUserId());
            
            var practiceStats = await this.questionRepositoryLegacy.GetPracticeStatsAsync(GetCurrentUserId());
            if(practiceStats != null)
            {
                practiceStats.TotalFlagged++;
                practiceStats.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                practiceStats = new PracticeStats
                {
                    Id = Guid.NewGuid().ToString(),
                    TotalFlagged = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            await this.questionRepositoryLegacy.CreateOrUpdatePracticeStats(practiceStats, GetCurrentUserId());
            return Ok();
        }

        [HttpGet("practicestats")]
        public async Task<PracticeStats> GetPracticeStats()
        {
            return await this.questionRepositoryLegacy.GetPracticeStatsAsync(GetCurrentUserId());
        }

        [HttpPost("filter")]
        public async Task<IEnumerable<QuestionViewModel>> FilterQuestions(QuestionsFilterRequest filterRequest)
        {
           return await this.practiceService.GetQuestionsLegacy(filterRequest, GetCurrentUserId());
        }

        [HttpPost("uploadimage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                string message = "No file was uploaded";
                this.logger.LogError(message);
                return BadRequest(message);
            }

            var blobUrl = await this.questionRepositoryLegacy.UploadQuestionImage(file);
            return Ok(new { imageUrl = blobUrl });
        }

        [HttpPost("familiaritycounts")]
        public Task<QuestionFamiliarityCounts> GetQuestionFamiliarityCounts([FromBody] FamiliarityCountsRequest request)
        {
            return this.practiceService.GetCategoryCounts(GetCurrentUserId(), request);
        }

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

        private async Task CreateQuestion(QuestionViewModel model, string questionCode, string currentUserId, string? questionId = null)
        {
            questionId = questionId ?? Guid.NewGuid().ToString();
            var question = new MedicLaunchApi.Data.Question
            {
                Id = questionId,
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<Data.QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                Options = model.Options.Select(m => new Data.AnswerOption()
                {
                    Id = Guid.NewGuid().ToString(),
                    Letter = m.Letter,
                    Text = m.Text,
                    QuestionId = questionId
                }).ToList(),
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                LearningPoints = model.LearningPoints,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = currentUserId,
                CreatedBy = currentUserId,
                Code = questionCode,
            };

            await this.questionRepository.CreateQuestionAsync(question);
        }
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
