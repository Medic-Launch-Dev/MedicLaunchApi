using MedicLaunchApi.Models;

namespace MedicLaunchApi.Common
{
    public class PaymentHelper
    {
        public static SubscriptionPlan GetSubscriptionPlan(string planId)
        {
            if (planId == null)
            {
                return null;
            }

            var plans = new SubscriptionPlan[] {
                new SubscriptionPlan()
                {
                    PlanId = "1",
                    LookupKey = "standard_monthly",
                    Amount = 1700, // amount is in cents, so this is £17.00
                    Months = 1
                },
                new SubscriptionPlan()
                {
                    PlanId = "2",
                    LookupKey = "standard_quarterly",
                    Amount = 2900, // £29
                    Months = 3
                },
                new SubscriptionPlan()
                {
                    PlanId = "3",
                    LookupKey = "standard_yearly",
                    Amount = 4200, // £42
                    Months = 12
                }
            };

            return plans.Where(m => m.PlanId == planId).FirstOrDefault();
        }
    }
}
