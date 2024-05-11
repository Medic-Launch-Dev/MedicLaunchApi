namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionFamiliarityCounts
    {
        public int NewQuestions { get; set; }

        public int IncorrectQuestions { get; set; }

        public int FlaggedQuestions { get; set; }

        public int AllQuestions { get; set; }
    }
}