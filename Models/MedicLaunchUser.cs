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

        public int GraduationYear { get; set; }

        public string? City { get; set; }

        public string? HowDidYouHearAboutUs { get; set; }

        public bool SubscribeToPromotions { get; set; }

        public string? SubscriptionPlanId { get; set; }

        public DateTime? SubscriptionCreatedDate { get; set; }

        public DateTime? SubscriptionExpiryDate { get; set; }

        public string SubscriptionStatus { get; set; }
    }
}
