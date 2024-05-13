namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionViewModel
    {
        public string? Id { get; set; }

        public string? QuestionCode { get; set; }

        public string SpecialityId { get; set; }

        public string QuestionType { get; set; }

        public string QuestionText { get; set; }

        public IEnumerable<OptionViewModel> Options { get; set; }

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string LearningPoints { get; set; }

        public bool IsSubmitted { get; set; }

        public string? SpecialityName { get; set; }

        public string? Note { get; set; }
    }

    public class OptionViewModel
    {
        public string Letter { get; set; }
        public string Text { get; set; }
    }
}
