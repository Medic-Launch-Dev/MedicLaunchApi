namespace MedicLaunchApi.Models.ViewModels
{
    public class QuestionViewModel
    {
        public string SpecialityId { get; set; }

        public string SpecialityName { get; set; }

        public string QuestionType { get; set; }

        public string QuestionText { get; set; }

        public string LabValues { get; set; }

        public IEnumerable<Option> Options { get; set; }

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string References { get; set; }

    }
}
