namespace MedicLaunchApi.Models.ViewModels
{
    public class MyUserProfile
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string University { get; set; }

        public int GraduationYear { get; set; }

        public string City { get; set; }

        public bool SubscribeToPromotions { get; set; }

        public string SubscriptionMonths { get; set; }

        public string SubscriptionPurchaseDate { get; set; }

        public int QuestionsCompleted { get; set; }

        public bool HasActiveSubscription { get; set; }

        public string PhoneNumber { get; set; }

        public bool IsOnFreeTrial { get; set; }

        public int? FreeTrialDaysRemaining { get; set; }

        public int RemainingTrialQuestions { get; set; }

        public int RemainingTrialClinicalCases { get; set; }

        public string? StripeSubscriptionStatus { get; set; }
    }
}
