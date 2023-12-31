namespace MedicLaunchApi.Models
{
    public class FlaggedQuestion
    {
        public string Id { get; set; }

        public string QuestionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
