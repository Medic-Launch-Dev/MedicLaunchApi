namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionsFilterResponse
    {
        public IEnumerable<QuestionViewModel> NewQuestions { get; set; }

        public IEnumerable<QuestionViewModel> IncorrectQuestions { get; set; }

        public IEnumerable<QuestionViewModel> FlaggedQuestions { get; set; }

        public IEnumerable<QuestionViewModel> AllQuestions { get; set; }
    }
}
