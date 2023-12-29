namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionsFilterRequest
    {
        public string[] SpecialityIds { get; set; }

        public QuestionType? QuestionType { get; set; }
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
