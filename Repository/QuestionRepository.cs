using MedicLaunchApi.Models;
using MedicLaunchApi.Storage;

namespace MedicLaunchApi.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AzureBlobClient azureBlobClient;

        public QuestionRepository(AzureBlobClient azureBlobClient)
        {
            this.azureBlobClient = azureBlobClient;
        }

        public async Task<Question> CreateQuestionAsync(Question question, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(question.SpecialityId, question.Id);
            return await azureBlobClient.CreateItemAsync(questionJsonPath, question, cancellationToken);
        }

        public async Task<Question> UpdateQuestionAsync(Question question, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(question.SpecialityId, question.Id);
            return await azureBlobClient.UpdateItemAsync(questionJsonPath, question, cancellationToken);
        }

        public async Task<IEnumerable<Question>> GetQuestionsAsync(string specialityId, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetAllQuestionsJsonPath(specialityId);
            return await azureBlobClient.GetAllItemsAsync<Question>(questionJsonPath, cancellationToken);
        }

        public Task DeleteQuestionAsync(string specialityId, string questionId, CancellationToken cancellationToken)
        {
            var questionJsonPath = GetQuestionJsonPath(specialityId, questionId);
            return azureBlobClient.DeleteItemAsync(questionJsonPath, cancellationToken);
        }

        public async Task AddQuestionAttempt(QuestionAttempt attempt, string userId)
        {
            string userAttemptsJsonPath = $"user/{userId}/questionattempts/{attempt.Id}.json";
            await azureBlobClient.CreateItemAsync(userAttemptsJsonPath, attempt, CancellationToken.None);
        }

        public async Task AddQuestionFlagged(FlaggedQuestion flaggedQuestion, string userId)
        {
            string userAttemptsJsonPath = $"user/{userId}/flaggedquestions/{flaggedQuestion.Id}.json";
            await azureBlobClient.CreateItemAsync(userAttemptsJsonPath, flaggedQuestion, CancellationToken.None);
        }

        private string GetQuestionJsonPath(string specialtyId, string questionId)
        {
            return $"speciality/{specialtyId}/questions/{questionId}.json";
        }

        private string GetAllQuestionsJsonPath(string specialtyId)
        {
            return $"speciality/{specialtyId}/questions";
        }
    }
}
