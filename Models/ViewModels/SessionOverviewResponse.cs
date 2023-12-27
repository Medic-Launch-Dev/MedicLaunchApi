namespace MedicLaunchApi.Models.ViewModels
{
    public class SessionOverviewResponse
    {
        public int TotalQuestions { get; set; }
        public IEnumerable<QuestionAttempt> AttemptedQuestions { get; set; }
    }
}
