namespace MedicLaunchApi.Models.ViewModels
{
    public class CreateFlashcardRequest
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string SpecialityId { get; set; }
    }

    public class UpdateFlashcardRequest
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string SpecialityId { get; set; }
    }

    public class FlashcardResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string SpecialityId { get; set; }

        public SpecialityViewModel Speciality { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
