using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicLaunchApi.Repository;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeSessionController: ControllerBase
    {
        private readonly ILogger<PracticeSessionController> logger;
        private readonly PracticeSessionRepository practiceSessionRepository;
        private readonly QuestionRepository questionRepository;

        public PracticeSessionController(ILogger<PracticeSessionController> logger,
            PracticeSessionRepository practiceSessionRepository,
            QuestionRepository questionRepository)
        {
            this.logger = logger;
            this.practiceSessionRepository = practiceSessionRepository;
            this.questionRepository = questionRepository;
        }

        [HttpPost("start")]
        public async Task<PracticeSessionResponse> StartPracticeSession(PracticeSessionRequest request)
        {
            var userId = GetCurrentUserId();
            var questions = await this.questionRepository.GetQuestionsAsync(request.SpecialityId, CancellationToken.None);
            var practiceSession = await this.practiceSessionRepository.CreatePracticeSession(request, userId, questions.Count(), CancellationToken.None);
            
            var practiceSessionResponse = new PracticeSessionResponse
            {
                Id = practiceSession.Id,
                Speciality = new SpecialityViewModel
                {
                    Id = request.SpecialityId,
                    Name = request.SpecialityName
                },
                AttemptedQuestions = new List<QuestionAttempt>(),
                Questions = questions.Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    SpecialityId = q.SpecialityId,
                    QuestionType = q.QuestionType.ToString(),
                    QuestionText = q.QuestionText,
                    LabValues = q.LabValues,
                    Options = q.Options,
                    CorrectAnswerLetter = q.CorrectAnswerLetter,
                    Explanation = q.Explanation,
                    ClinicalTips = q.ClinicalTips,
                    References = q.References
                }),
                TotalQuestions = questions.Count(),
                Completed = false
            };

            return practiceSessionResponse;
        }

        [HttpPost("attemptquestion")]
        public async Task<IActionResult> AttemptQuestion(string practiceSessionId, QuestionAttemptRequest questionAttempt)
        {
            await this.practiceSessionRepository.AttemptQuestion(practiceSessionId, GetCurrentUserId(), questionAttempt, CancellationToken.None);
            return Ok();
        }

        [HttpGet("sessionoverview")]
        public async Task<SessionOverviewResponse> GetSessionOverview(string practiceSessionId)
        {
            var session = await this.practiceSessionRepository.GetPracticeSession(practiceSessionId, GetCurrentUserId(), CancellationToken.None);
            return new SessionOverviewResponse
            {
                TotalQuestions = session.TotalQuestions,
                AttemptedQuestions = session.AttemptedQuestions
            };
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
