namespace MedicLaunchApi.Models
{
    public class Student
    {
        public string Id { get; set; }

        public IEnumerable<QuestionAttempt> QuestionsAttempted { get; set; }
    }
}
