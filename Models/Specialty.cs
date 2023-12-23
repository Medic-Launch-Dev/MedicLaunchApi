namespace MedicLaunchApi.Models
{
    public class Specialty
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Question> Questions { get; set; }
    }
}
