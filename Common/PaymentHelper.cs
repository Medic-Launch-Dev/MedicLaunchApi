using MedicLaunchApi.Models;

namespace MedicLaunchApi.Common
{
    public class PaymentHelper
    {
        public static SubscriptionPlan GetSubscriptionPlan(string planId)
        {
            var plans = new SubscriptionPlan[] {
                new SubscriptionPlan()
                {
                    PlanId = "1",
                    Amount = 2000,
                    Months = 1
                },
                new SubscriptionPlan()
                {
                    PlanId = "2",
                    Amount = 5000,
                    Months = 2
                },
                new SubscriptionPlan()
                {
                    PlanId = "3",
                    Amount = 10000,
                    Months = 3
                }
            };

            return plans.Where(m => m.PlanId == planId).FirstOrDefault();
        }
    }
}
