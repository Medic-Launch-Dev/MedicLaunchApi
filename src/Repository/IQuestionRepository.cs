using MedicLaunchApi.Models;

namespace MedicLaunchApi.Repository
{
    public interface IQuestionRepository
    {
        Task<Question> CreateQuestionAsync(Question question, CancellationToken cancellationToken);
    }
}