namespace MedicLaunchApi.Models
{
    public class UserProfile
    {
        public string UserId { get; set; }

        public string? StripeCustomerId { get; set; }

        public string? SubscriptionPlanId { get; set; }

        public DateTime? SubscriptionCreatedDate { get; set; }

        public DateTime? SubscriptionExpiryDate { get; set; }
    }
}
