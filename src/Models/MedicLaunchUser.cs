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

        public DateTime? CreatedOn { get; set; }

        public bool IsOnFreeTrial => CreatedOn.HasValue &&
            CreatedOn.Value.AddDays(7) > DateTime.UtcNow &&
            !HasActiveSubscription;

        public bool HasActiveSubscription => SubscriptionExpiryDate.HasValue &&
            SubscriptionExpiryDate.Value > DateTime.UtcNow;

        public int? FreeTrialDaysRemaining
        {
            get
            {
                if (!CreatedOn.HasValue || HasActiveSubscription)
                    return null;

                var trialEnd = CreatedOn.Value.AddDays(7);
                var now = DateTime.UtcNow;

                if (now > trialEnd)
                    return null;

                return (int)Math.Ceiling((trialEnd - now).TotalDays);
            }
        }

        public int TrialQuestionsAttemptedCount { get; set; }
        
        public int TrialClinicalCasesGeneratedCount { get; set; }
    }
}
