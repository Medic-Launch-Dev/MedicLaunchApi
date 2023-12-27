using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Storage;

namespace MedicLaunchApi.Repository
{
    public class PracticeSessionRepository(AzureBlobClient azureBlobClient, ILogger<PracticeSessionRepository> logger)
    {
        private readonly AzureBlobClient azureBlobClient = azureBlobClient;
        private readonly ILogger<PracticeSessionRepository> logger = logger;

        public async Task<PracticeSession> CreatePracticeSession(PracticeSessionRequest request, string userId, int totalQuestions, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Creating practice session");
            var practiceSession = new PracticeSession()
            {
                Id = Guid.NewGuid().ToString(),
                SpecialityId = request.SpecialityId,
                AttemptedQuestions = new List<QuestionAttempt>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Completed = false,
                TotalQuestions = totalQuestions
            };

            var userSessionBlobPath = GetUserSessionJsonPath(userId, practiceSession.Id);
            return await azureBlobClient.CreateItemAsync<PracticeSession>(userSessionBlobPath, practiceSession, cancellationToken);
        }

        public async Task AttemptQuestion(string practiceSessionId, string userId, QuestionAttemptRequest questionAttempt, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Attempting question {questionAttempt.QuestionId}");
            var userSessionBlobPath = GetUserSessionJsonPath(userId, practiceSessionId);
            var practiceSession = await azureBlobClient.GetItemAsync<PracticeSession>(userSessionBlobPath, cancellationToken, false);

            if (practiceSession.AttemptedQuestions.Count() == 0)
            {
                practiceSession.AttemptedQuestions = new List<QuestionAttempt>();
            }

            practiceSession.AttemptedQuestions.ToList().Add(new QuestionAttempt
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = questionAttempt.QuestionId,
                ChosenOption = questionAttempt.AnswerLetter,
                IsCorrect = questionAttempt.IsCorrect,
                CreatedAt = DateTime.UtcNow,
                IsFlagged = questionAttempt.IsFlagged,
                UpdatedAt = DateTime.UtcNow
            });

            // Save session
            await azureBlobClient.UpdateItemAsync<PracticeSession>(userSessionBlobPath, practiceSession, cancellationToken);
        }

        public async Task<PracticeSession> GetPracticeSession(string practiceSessionId, string userId, CancellationToken cancellationToken)
        {
            var userSessionBlobPath = GetUserSessionJsonPath(userId, practiceSessionId);
            return await azureBlobClient.GetItemAsync<PracticeSession>(userSessionBlobPath, cancellationToken, false);
        }

        public async Task<SessionOverviewResponse> GetSessionOverview(string practiceSessionId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private string GetUserSessionJsonPath(string userId, string practiceSessionId)
        {
            return $"user/{userId}/sessions/{practiceSessionId}.json";
        }
    }
}
