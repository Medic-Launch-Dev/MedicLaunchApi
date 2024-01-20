namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionViewModel
    {
        public string? Id { get; set; }

        public string? QuestionCode { get; internal set; }

        public string SpecialityId { get; set; }

        public string QuestionType { get; set; }

        public string QuestionText { get; set; }

        public IEnumerable<Option> Options { get; set; }

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string LearningPoints { get; set; }

        public bool IsSubmitted { get; set; }
    }
}
