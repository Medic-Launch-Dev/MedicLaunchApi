namespace MedicLaunchApi.Models
{
    public class Question
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string SpecialityId { get; set; }

        public QuestionType QuestionType { get; set; }

        public string QuestionText { get; set; }

        public string LabValues { get; set; }

        public IEnumerable<string> Options { get; set; }

        public string CorrectAnswerLetter { get; set; }

        public string Explanation { get; set; }

        public string ClinicalTips { get; set; }

        public string References { get; set; }

        public string AuthorUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string UpdatedByUserId { get; set; }
    }

    public enum QuestionType
    {
        General,
        PaperOneMockExam,
        PaperTwoMockExam
    }
}
