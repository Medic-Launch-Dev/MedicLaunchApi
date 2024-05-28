namespace MedicLaunchApi.Models.ViewModels
{
    public class MyUserProfile
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string University { get; set; }

        public int GraduationYear { get; set; }

        public string City { get; set; }

        public bool SubscribeToPromotions { get; set; }

        public string SubscriptionMonths { get; set; }

        public string SubscriptionPurchaseDate { get; set; }

        public int QuestionsCompleted { get; set; }
    }
}
