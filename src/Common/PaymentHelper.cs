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
                    StripePriceId = "price_1QIjrOJUITqc1TPfCumWSsVB",
                    Amount = 1700, // amount is in cents, so this is £17.00
                    Months = 1
                },
                new SubscriptionPlan()
                {
                    PlanId = "2",
                    StripePriceId = "price_1PrUcmJUITqc1TPfpYpki2Sk",
                    Amount = 2900, // £29
                    Months = 3
                },
                new SubscriptionPlan()
                {
                    PlanId = "3",
                    StripePriceId = "price_1QIjrhJUITqc1TPf67H4KCC6",
                    Amount = 4200, // £42
                    Months = 12
                }
            };

            return plans.Where(m => m.PlanId == planId).FirstOrDefault();
        }
    }
}
