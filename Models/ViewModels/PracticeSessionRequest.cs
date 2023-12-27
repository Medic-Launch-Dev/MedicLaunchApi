namespace MedicLaunchApi.Models.ViewModels
{
    public class PracticeSessionRequest
    {
        // TODO: ask about quantity (how do they know question numbers in the custom field)
        public string SpecialityId { get; internal set; }

        public string SpecialityName { get; set; }

        public FamiliarityLevel FamiliarityLevel { get; set; }

        public SelectionOrder SelectionOrder { get; set; }

        public int TotalQuestionsRequested { get; set; }
    }

    public enum FamiliarityLevel
    {
        NewQuestions,
        IncorrectQuestions,
        AllQuestions,
        FlaggedQuestions
    }

    public enum SelectionOrder
    {
        Random,
        OrderedBySpeciality,
    }
}
