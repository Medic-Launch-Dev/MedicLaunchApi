using MedicLaunchApi.Common;
using MedicLaunchApi.Controllers;
using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Identity;
using Stripe;

namespace MedicLaunchApi.Services
{
    public class PaymentService
    {
        private readonly string stripeApiKey;
        private readonly string stripePublishableKey;
        private readonly ILogger<PaymentController> logger;
        private readonly UserManager<MedicLaunchUser> userManager;

        public PaymentService(ILogger<PaymentController> logger, UserManager<MedicLaunchUser> userManager)
        {
            this.logger = logger;
            this.stripeApiKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY") ?? string.Empty;

            if (string.IsNullOrEmpty(this.stripeApiKey))
            {
                throw new Exception("STRIPE_API_KEY environment variable is not set");
            }

            this.stripePublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY") ?? string.Empty;
            if (string.IsNullOrEmpty(this.stripePublishableKey))
            {
                throw new Exception("STRIPE_PUBLISHABLE_KEY environment variable is not set");
            }

            this.userManager = userManager;
        }

        public async Task<string> CreatePaymentIntent(string planId, string userId)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var user = await this.userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    this.logger.LogError($"User not found with id {userId}");
                }
                else
                {
                    user.SubscriptionPlanId = planId;
                    await this.userManager.UpdateAsync(user);
                }

                var customerOptions = new CustomerListOptions { Limit = 10, Email = user.Email! };
                var customerService = new CustomerService();
                Customer customer = customerService.List(customerOptions).SingleOrDefault();
                if (customer == null)
                {
                    this.logger.LogError($"Customer not found with email {user.Email}");
                }

                var subscription = PaymentHelper.GetSubscriptionPlan(planId);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = subscription.Amount,
                    Currency = "GBP",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Customer = customer.Id,
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = service.Create(options);

                return paymentIntent.ClientSecret;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating payment intent");
                throw;
            }
        }

        public void CreateStripeCustomer(MedicLaunchUser user)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var options = new CustomerCreateOptions
                {
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email
                };

                var service = new CustomerService();
                service.Create(options);

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating stripe customer");
            }
         }

        public string GetPublishKey()
        {
            return this.stripePublishableKey;
        }
    }
}
