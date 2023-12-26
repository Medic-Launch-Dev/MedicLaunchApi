namespace MedicLaunchApi.Models
{
    public class QuestionImage
    {
        public string Id { get; set; }

        public string QuestionId { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
