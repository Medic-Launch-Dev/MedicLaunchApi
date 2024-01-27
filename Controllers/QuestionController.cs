using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/questions")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly ILogger<QuestionController> logger;
        private readonly QuestionRepository questionRepository;
        private readonly PracticeService practiceService;

        public QuestionController(ILogger<QuestionController> logger, QuestionRepository questionRepository, PracticeService practiceService)
        {
            this.logger = logger;
            this.questionRepository = questionRepository;
            this.practiceService = practiceService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] QuestionViewModel model)
        {
            string currentUserId = GetCurrentUserId();
            string questionCode = await GetQuestionCode(model.SpecialityId, CancellationToken.None);
            await CreateQuestion(model, questionCode, currentUserId);

            return Ok();
        }

        [HttpPost("update/{questionId}")]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionViewModel model, string questionId)
        {
            string currentUserId = GetCurrentUserId();

            // If specialty is changed, we should delete question from the previous speciality and create a new question under the new speciality
            if (model.PreviousSpecialityId != model.SpecialityId)
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
                Options = model.Options,
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                LearningPoints = model.LearningPoints,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserId,
                Code = model.QuestionCode,
            };

            var updatedQuestion = await this.questionRepository.UpdateQuestionAsync(question, CancellationToken.None);
            return Ok(updatedQuestion);
        }

        [HttpGet("speciality/{specialityId}")]
        public async Task<IEnumerable<QuestionViewModel>> GetQuestions(string specialityId)
        {
            var questions = await this.questionRepository.GetQuestionsAsync(specialityId, CancellationToken.None);
            return CreateQuestionViewModel(questions);
        }


        [HttpDelete("delete/{specialityId}/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(string specialityId, string questionId)
        {
            await this.questionRepository.DeleteQuestionAsync(specialityId, questionId, CancellationToken.None);
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
            await this.questionRepository.AddSpeciality(speciality, CancellationToken.None);
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
            return await this.questionRepository.GetSpecialities(CancellationToken.None);
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(QuestionAttemptRequest questionAttempt)
        {
            var attempt = new QuestionAttempt
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = questionAttempt.QuestionId,
                ChosenAnswer = questionAttempt.ChosenAnswer,
                CorrectAnswer = questionAttempt.CorrectAnswer,
                IsCorrect = questionAttempt.IsCorrect,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await this.questionRepository.AddQuestionAttempt(attempt, GetCurrentUserId());

            var practiceStats = await this.questionRepository.GetPracticeStatsAsync(GetCurrentUserId());
            if (practiceStats != null)
            {
                if (attempt.IsCorrect)
                {
                    practiceStats.TotalCorrect++;
                }
                else
                {
                    practiceStats.TotalIncorrect++;
                }

                practiceStats.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                practiceStats = new PracticeStats
                {
                    Id = Guid.NewGuid().ToString(),
                    TotalCorrect = attempt.IsCorrect ? 1 : 0,
                    TotalIncorrect = attempt.IsCorrect ? 0 : 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            await this.questionRepository.CreateOrUpdatePracticeStats(practiceStats, GetCurrentUserId());

            return Ok();
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

            await this.questionRepository.AddQuestionFlagged(questionFlagged, GetCurrentUserId());
            
            var practiceStats = await this.questionRepository.GetPracticeStatsAsync(GetCurrentUserId());
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

            await this.questionRepository.CreateOrUpdatePracticeStats(practiceStats, GetCurrentUserId());
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
           return await this.practiceService.GetQuestions(filterRequest, GetCurrentUserId());
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

            var blobUrl = await this.questionRepository.UploadQuestionImage(file);
            return Ok(new { imageUrl = blobUrl });
        }

        private IEnumerable<QuestionViewModel> CreateQuestionViewModel(IEnumerable<Question> questions)
        {
            return questions.Select(q => new QuestionViewModel
            {
                Id = q.Id,
                SpecialityId = q.SpecialityId,
                QuestionType = q.QuestionType.ToString(),
                QuestionText = q.QuestionText,
                Options = q.Options,
                CorrectAnswerLetter = q.CorrectAnswerLetter,
                Explanation = q.Explanation,
                ClinicalTips = q.ClinicalTips,
                LearningPoints = q.LearningPoints
            }).ToList();
        }

        private async Task CreateQuestion(QuestionViewModel model, string questionCode, string currentUserId, string? questionId = null)
        {
            // TODO: add question code
            var question = new Question
            {
                Id = questionId ?? Guid.NewGuid().ToString(),
                SpecialityId = model.SpecialityId,
                QuestionType = Enum.Parse<QuestionType>(model.QuestionType),
                QuestionText = model.QuestionText,
                Options = model.Options,
                CorrectAnswerLetter = model.CorrectAnswerLetter,
                Explanation = model.Explanation,
                ClinicalTips = model.ClinicalTips,
                LearningPoints = model.LearningPoints,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserId,
                AuthorUserId = currentUserId,
                Code = questionCode,
            };

            await this.questionRepository.CreateQuestionAsync(question, CancellationToken.None);
        }

        private async Task<string> GetQuestionCode(string specialityId, CancellationToken token)
        {
            // get count of questions in the speciality
            var questions = await this.questionRepository.GetQuestionsAsync(specialityId, token);
            var allSpecialities = await this.questionRepository.GetSpecialities(token);
            var speciality = allSpecialities.FirstOrDefault(s => s.Id == specialityId);
            if (speciality == null)
            {
                throw new Exception("Speciality not found");
            }

            string questionCode = speciality.Name.Substring(0, 2).ToUpper() + (questions.Count() + 1);
            return questionCode;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
