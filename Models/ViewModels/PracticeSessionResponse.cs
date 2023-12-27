namespace MedicLaunchApi.Models.ViewModels
{
    public class PracticeSessionResponse
    {
        public string Id { get; set; }

        public IEnumerable<QuestionViewModel> Questions { get; set; }

        public SpecialityViewModel Speciality { get; set; }

        public IEnumerable<QuestionAttempt> AttemptedQuestions { get; set; }

        public int TotalQuestions { get; set; }

        public bool Completed { get; set; }
    }
}
