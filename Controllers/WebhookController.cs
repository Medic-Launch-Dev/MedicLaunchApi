using MedicLaunchApi.Common;
using MedicLaunchApi.Models;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace MedicLaunchApi.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController: ControllerBase
    {
        private readonly string stripeWebhookSecret;
        private readonly ILogger<PaymentController> logger;
        private readonly UserManager<MedicLaunchUser> userManager;

        public WebhookController(ILogger<PaymentController> logger, UserManager<MedicLaunchUser> userManager, PaymentRepository paymentRepository, UserRepository userRepository)
        {
            this.logger = logger;

            this.stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") ?? string.Empty;
            if (string.IsNullOrEmpty(this.stripeWebhookSecret))
            {
                throw new Exception("STRIPE_WEBHOOK_SECRET environment variable is not set");
            }

            this.userManager = userManager;
        }

        [HttpPost]
        [Route("stripe")]
        public async Task<ActionResult> StripeHook()
        {
            try
            {
                PaymentIntent? intent = null;
                Event stripeEvent;
                try
                {
                    var json = await new StreamReader(Request.Body).ReadToEndAsync();

                    // TODO: remove throwOnApiVersionMismatch: false later
                    stripeEvent = EventUtility.ConstructEvent(json,
                        Request.Headers["Stripe-Signature"], this.stripeWebhookSecret, throwOnApiVersionMismatch: false);


                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Unable to parse stripe web hook event");
                    return BadRequest();
                }

                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        intent = stripeEvent.Data.Object as PaymentIntent;

                        await HandlePaymentSucceeded(intent);
                        break;
                    case Events.PaymentIntentPaymentFailed:
                        logger.LogInformation("Payment Failure: {ID}. Details {Details}", intent?.Id, stripeEvent.ToJson());

                        // Notify the customer that payment failed ?

                        break;
                    default:
                        // Handle other event types

                        break;
                }
                return new EmptyResult();

            }
            catch (StripeException ex)
            {
                // Invalid Signature
                this.logger.LogError(ex, "Stripe Exception");
                return BadRequest(ex.Message);
            }
        }

        private async Task<ActionResult> HandlePaymentSucceeded(PaymentIntent intent)
        {
            if (intent == null)
            {
                this.logger.LogError("Invalid intent");
                return BadRequest();
            }

            if (intent.Customer?.Email == null)
            {
                this.logger.LogError("Invalid customer email");
                return BadRequest();
            }

            string customerEmail = intent.Customer.Email;

            // Find user by email using usermanager
            var user = await userManager.FindByEmailAsync(customerEmail);
            if (user == null)
            {
                this.logger.LogError($"Unable to find user with email {customerEmail}");
                return BadRequest();
            }

            var plan = PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId!);
            user.SubscriptionExpiryDate = DateTime.UtcNow.AddMonths(plan.Months);
            user.SubscriptionCreatedDate = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return new EmptyResult();
        }
    }
}
