namespace MedicLaunchApi.Models
{
    public class PracticeStats
    {
        public string Id { get; set; }

        public int TotalQuestions { get; set; }

        public int TotalCorrect { get; set; }

        public int TotalIncorrect { get; set; }

        public int TotalFlagged { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
