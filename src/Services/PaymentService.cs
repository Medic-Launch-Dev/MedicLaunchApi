using MedicLaunchApi.Common;
using MedicLaunchApi.Controllers;
using MedicLaunchApi.Models;
using Microsoft.AspNetCore.Identity;
using Stripe;
using Stripe.Checkout;

namespace MedicLaunchApi.Services
{
    public class PaymentService
    {
        private readonly string stripeApiKey;
        private readonly string stripePublishableKey;
        private readonly ILogger<PaymentController> logger;
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly IConfiguration configuration;

        public PaymentService(ILogger<PaymentController> logger, UserManager<MedicLaunchUser> userManager, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
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

                var customerOptions = new CustomerListOptions { Limit = 1, Email = user.Email! };
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(customerOptions);
                var customer = customers.FirstOrDefault();
                if (customer == null)
                {
                    this.logger.LogError($"Customer not found with email {user.Email}");
                    throw new Exception("Stripe customer not found");
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

        public async Task<string> CreateCheckoutSession(string planId, string userId)
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

                var customerOptions = new CustomerListOptions { Limit = 1, Email = user.Email! };
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(customerOptions);
                var customer = customers.FirstOrDefault();
                if (customer == null)
                {
                    this.logger.LogError($"Customer not found with email {user.Email}");
                    throw new Exception("Stripe customer not found");
                }

                var subscription = PaymentHelper.GetSubscriptionPlan(planId);

                var priceService = new PriceService();
                var price = await priceService.ListAsync(new PriceListOptions
                {
                    LookupKeys = new List<string> { subscription.LookupKey },
                    Limit = 1,
                });

                if (!price.Any())
                {
                    this.logger.LogError($"Price not found for lookup key {subscription.LookupKey}");
                    throw new Exception("Stripe price not found");
                }

                var domain = configuration.GetValue<string>("ReactApp:Url");
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    Price = price.First().Id,
                    Quantity = 1,
                  },
                },
                    Mode = "payment",
                    SuccessUrl = domain + "/payment-complete",
                    CancelUrl = domain + "/subscribe",
                    AllowPromotionCodes = true,
                    Customer = customer.Id,
                };
                var service = new SessionService();
                Session session = service.Create(options);

                return session.Url;
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
