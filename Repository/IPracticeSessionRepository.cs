using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;

namespace MedicLaunchApi.Repository
{
    public interface IPracticeSessionRepository
    {
        Task AttemptQuestion(string practiceSessionId, QuestionAttempt questionAttempt, CancellationToken token);
        Task<PracticeSession> CreatePracticeSession(PracticeSessionRequest request, string userId, CancellationToken token);
        Task<SessionOverviewResponse> GetSessionOverview(string practiceSessionId, CancellationToken token);
    }
}