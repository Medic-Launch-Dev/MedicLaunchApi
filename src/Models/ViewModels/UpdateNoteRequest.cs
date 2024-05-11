namespace MedicLaunchApi.Models.ViewModels
{
    public class UpdateNoteRequest
    {
        public string Id { get; set; }

        public string SpecialityId { get; set; }

        public string Content { get; set; }
    }
}
