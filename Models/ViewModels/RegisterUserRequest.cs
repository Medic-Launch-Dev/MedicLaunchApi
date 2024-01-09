namespace MedicLaunchApi.Models.ViewModels
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public string GraduationYear { get; set; } = string.Empty;

        public string? City { get; set; }

        public string? HowDidYouHearAboutUs { get; set; }
    }
}
