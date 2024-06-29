namespace MedicLaunchApi.Models.ViewModels
{
    public class SpecialityAnalyzerResponse
    {
        public string SpecialityId { get; set; }

        public string SpecialityName { get; set; }

        public int QuestionsAnswered { get; set; }

        public int TotalQuestions { get; set; }

        public int Correct { get; set; }

        public int Incorrect { get; set; }
    }
}
