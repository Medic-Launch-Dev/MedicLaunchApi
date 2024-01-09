using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MedicLaunchApi.Models
{
    public class MedicLaunchUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string University { get; set; }

        public string GraduationYear { get; set; }

        public string? City { get; set; }

        public string? HowDidYouHearAboutUs { get; set; }
    }
}
