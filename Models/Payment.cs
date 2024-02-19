namespace MedicLaunchApi.Models
{
    public class Payment
    {
        public string Id { get; set; }

        public string SubscriptionPlanId { get; set; }

        public string PaymentIntentId { get; set; }

        public string PaymentIntentClientSecret { get; set; }

        public string PaymentIntentStatus { get; set; }

        public long PaymentIntentAmount { get; set; }

        public string PaymentIntentCurrency { get; set; }
    }
}
