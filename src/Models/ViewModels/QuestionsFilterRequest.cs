namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionsFilterRequest
    {
        public string[] SpecialityIds { get; set; }

        public string QuestionType { get; set; }

        public Familiarity Familiarity { get; set; }

        public SelectionOrder SelectionOrder { get; set; }

        public bool AllSpecialitiesSelected { get; set; }

        public int Amount { get; set; } // Number of questions to return
    }

    public enum Familiarity
    {
        NewQuestions,
        IncorrectQuestions,
        FlaggedQuestions,
        AllQuestions
    }

    public enum SelectionOrder
    {
        Randomized,
        OrderBySpeciality
    }
}
