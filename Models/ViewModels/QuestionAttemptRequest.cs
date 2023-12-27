namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionAttemptRequest
    {
        public string QuestionId { get; set; }

        public string AnswerLetter { get; set; }

        public bool IsCorrect { get; set; }

        public bool IsFlagged { get; set; }
    }
}
