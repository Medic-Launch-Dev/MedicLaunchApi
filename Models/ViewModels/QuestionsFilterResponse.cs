namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionsFilterResponse
    {
        public IEnumerable<Question> NewQuestions { get; set; }

        public IEnumerable<Question> IncorrectQuestions { get; set; }

        public IEnumerable<Question> FlaggedQuestions { get; set; }

        public IEnumerable<Question> AllQuestions { get; set; }
    }
}
