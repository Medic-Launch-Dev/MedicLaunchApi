namespace MedicLaunchApi.Models
{
    public class QuestionAttempt
    {
        public string Id { get; set; }

        public string SpecialityId { get; set; }

        public string QuestionId { get; set; }

        public string ChosenAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
