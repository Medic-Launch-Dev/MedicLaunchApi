namespace MedicLaunchApi.Models.ViewModels
{
    public class CreateNoteRequest
    {
        public string? SpecialityId { get; set; }

        public string? QuestionId { get; set; }

        public string? FlashcardId { get; set; }

        public string Content { get; set; }
    }
}
