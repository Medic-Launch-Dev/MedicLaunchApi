namespace MedicLaunchApi.Models.ViewModels
{
    public class UpdateUserRequest
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string University { get; set; }

        public int GraduationYear { get; set; }

        public string City { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
