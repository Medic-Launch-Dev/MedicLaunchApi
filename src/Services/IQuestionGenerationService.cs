using MedicLaunchApi.Models.QuestionDTOs;

namespace MedicLaunchApi.Services
{
    public interface IQuestionGenerationService
    {
        Task<QuestionTextAndExplanation> GenerateQuestionTextAndExplanationAsync(string conditions);
        Task<string> GenerateLearningPointsAsync(string condition);
        Task<string> GenerateClinicalTipsAsync(string condition);
    }
}