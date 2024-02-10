using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly string stripeApiKey;
        private readonly string stripePublishableKey;
        public PaymentController()
        {
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
        }

        [HttpPost]
        [Route("create-payment-intent")]
        public IActionResult CreatePaymentIntent(string paymentPlan)
        {
            // TODO: Add logic to determine the amount based on the payment plan

            StripeConfiguration.ApiKey = this.stripeApiKey;

            var options = new PaymentIntentCreateOptions
            {
                Amount = 2000,
                Currency = "GBP", // TODO: Change to the currency of the user
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = service.Create(options);
            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }

        [HttpGet]
        [Route("publishable-key")]
        public IActionResult GetPublishableKey()
        {
            return Ok(new { publishableKey = this.stripePublishableKey });
        }
    }
}
