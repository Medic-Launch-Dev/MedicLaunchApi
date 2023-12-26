namespace MedicLaunchApi.Models
{
    public class QuestionAttempt
    {
        public string Id { get; set; }

        public string QuestionId { get; set; }

        public string AnswerLetter { get; set; }

        public bool IsCorrect { get; set; }

        public bool IsFlagged { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
