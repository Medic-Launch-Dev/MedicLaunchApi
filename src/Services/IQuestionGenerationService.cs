using MedicLaunchApi.Models.QuestionDTOs;

namespace MedicLaunchApi.Services
{
    public interface IQuestionGenerationService
    {
        Task<QuestionTextAndExplanation> GenerateQuestionTextAndExplanationAsync(string conditions);
    }
}