namespace MedicLaunchApi.Models.ViewModels
{
    public class EditQuestionsRequest
    {
        public string SpecialityId { get; set; }

        public string QuestionType { get; set; } = "General";
    }
}