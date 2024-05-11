namespace MedicLaunchApi.Models.ViewModels
{
    // Used by admin only
    public class AddUserRequest
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public int GraduationYear { get; set; }

        public string City { get; set; }

        public string SubscriptionPlanId { get; set; }
    }
}
