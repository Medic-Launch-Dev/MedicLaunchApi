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

        public async Task<string> CreateCheckoutSession(string planLookupKey, string userId)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var user = await this.userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    this.logger.LogError($"User not found with id {userId}");
                    throw new Exception("User not found");
                }

                var customerOptions = new CustomerListOptions { Limit = 1, Email = user.Email! };
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(customerOptions);
                var customer = customers.FirstOrDefault();
                if (customer == null)
                {
                    var createdCustomer = await this.CreateStripeCustomerIfNotExists(user);
                    customer = createdCustomer;
                }

                var priceService = new PriceService();
                var price = await priceService.ListAsync(new PriceListOptions
                {
                    LookupKeys = new List<string> { planLookupKey },
                    Limit = 1,
                });

                if (!price.Any())
                {
                    this.logger.LogError($"Price not found for lookup key {planLookupKey}");
                    throw new Exception("Stripe price not found");
                }

                var domain = configuration.GetValue<string>("ReactApp:Url");
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                    {
                        new Stripe.Checkout.SessionLineItemOptions
                        {
                            Price = price.First().Id,
                            Quantity = 1,
                        },
                    },
                    Mode = "subscription",
                    SuccessUrl = domain + "/payment-complete",
                    CancelUrl = domain + "/subscribe",
                    AllowPromotionCodes = true,
                    Customer = customer.Id,
                };
                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = service.Create(options);

                return session.Url;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating checkout session");
                throw;
            }
        }

        public async Task<string> CreatePortalSession(string userId)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var user = await this.userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    this.logger.LogError($"User not found with id {userId}");
                    throw new Exception("User not found");
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

                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customer.Id,
                    ReturnUrl = configuration.GetValue<string>("ReactApp:Url") + "/my-profile",
                };
                var service = new Stripe.BillingPortal.SessionService();
                Stripe.BillingPortal.Session session = service.Create(options);

                return session.Url;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating billing portal session");
                throw;
            }
        }

        /// <summary>
        /// Creates a Stripe customer if it does not already exist for the given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Existing or newly created stripe customer</returns>
        public async Task<Customer> CreateStripeCustomerIfNotExists(MedicLaunchUser user)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var customerService = new CustomerService();

                var customers = await customerService.ListAsync(new CustomerListOptions
                {
                    Email = user.Email,
                    Limit = 1
                });
                var existingCustomer = customers.FirstOrDefault();
                if (existingCustomer != null)
                {
                    return existingCustomer;
                }

                var options = new CustomerCreateOptions
                {
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email
                };

                return await customerService.CreateAsync(options);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating stripe customer");
                throw;
            }
        }

        public async Task DeleteStripeCustomer(MedicLaunchUser user)
        {
            try
            {
                StripeConfiguration.ApiKey = this.stripeApiKey;
                var customerService = new CustomerService();
                var customers = await customerService.ListAsync(new CustomerListOptions { Email = user.Email, Limit = 1 });
                var customer = customers.FirstOrDefault();
                if (customer != null)
                {
                    await customerService.DeleteAsync(customer.Id);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error deleting Stripe customer");
                throw;
            }
        }

        public string GetPublishKey()
        {
            return this.stripePublishableKey;
        }
    }
}
