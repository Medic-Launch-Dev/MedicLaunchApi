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

        private string GetQuestionJsonPath(string specialtyId, string questionId)
        {
            return $"speciality/{specialtyId}/questions/{questionId}.json";
        }

        private string GetAllQuestionsJsonPath(string specialtyId)
        {
            return $"{specialtyId}/questions";
        }
    }
}
