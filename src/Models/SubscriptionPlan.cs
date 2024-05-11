namespace MedicLaunchApi.Models
{
    public class SubscriptionPlan
    {
        public string PlanId { get; set; }

        public long Amount { get; set; }

        public int Months { get; set; }
    }
}
