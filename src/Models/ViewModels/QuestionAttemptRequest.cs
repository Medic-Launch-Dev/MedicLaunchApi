namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionAttemptRequest
    {
        public string QuestionId { get; set; }

        public string ChosenAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

    }
}
