using Microsoft.AspNetCore.Identity;

namespace MedicLaunchApi.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string University { get; set; }

        public string GraduationYear { get; set; }

        public string City { get; set; }

        public string HowDidYouHearAboutUs { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
