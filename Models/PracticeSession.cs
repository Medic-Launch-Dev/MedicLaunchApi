namespace MedicLaunchApi.Models
{
    public class PracticeSession
    {
        public string Id { get; set; }

        public string SpecialityId { get; set; }

        public IEnumerable<QuestionAttempt> AttemptedQuestions { get; set; }

        public bool Completed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public int TotalQuestions { get; internal set; }
    }
}
