using MedicLaunchApi.Common;
using MedicLaunchApi.Models;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;

namespace MedicLaunchApi.Controllers
{
    [Route("api/webhook")]
    [ApiController]
    public class WebhookController: ControllerBase
    {
        private readonly string stripeWebhookSecret;
        private readonly ILogger<PaymentController> logger;
        private readonly PaymentRepository paymentRepository;
        private readonly UserRepository userRepository;
        private readonly UserManager<MedicLaunchUser> userManager;

        public WebhookController(ILogger<PaymentController> logger, UserManager<MedicLaunchUser> userManager, PaymentRepository paymentRepository, UserRepository userRepository)
        {
            this.logger = logger;

            this.stripeWebhookSecret = "whsec_4336112a6ad48ab2bc2dba791ef9187d30ddc55e455c34d46f1b040be530b5dd";

            this.userManager = userManager;
            this.paymentRepository = paymentRepository;
            this.userRepository = userRepository;
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
                return BadRequest();
            }
        }

        private async Task<ActionResult> HandlePaymentSucceeded(PaymentIntent intent)
        {
            if (intent == null)
            {
                this.logger.LogError("Invalid intent");
                return BadRequest();
            }

            //if(intent.Customer?.Email == null)
            //{
            //    this.logger.LogError("Invalid customer email");
            //    return BadRequest();
            //}

            string customerEmail = "khalid.abdilahi91@gmail.com"; //intent.Customer.Email;

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

            //var userId = user.Id;

            //var payments = await this.paymentRepository.GetPayments(userId, CancellationToken.None);
            //var storedIntent = payments.Where(m => m.PaymentIntentId == intent.Id).FirstOrDefault();

            //if (storedIntent == null)
            //{
            //    this.logger.LogError($"Unable to find corresponding payment object for the intent {intent.Id}");
            //    return BadRequest();
            //}

            //await this.paymentRepository.UpdatePaymentStatus(userId, intent.Id, intent.Status, CancellationToken.None);
            //var userProfile = await this.userRepository.GetUserProfile(userId, CancellationToken.None);

            //logger.LogInformation("Payment Succeeded: {ID}", intent.Id);

            //// TODO: Do we need to update customer that payment succeeded?
            //userProfile.SubscriptionCreatedDate = DateTime.UtcNow;

            //var subscription = PaymentHelper.GetSubscriptionPlan(storedIntent.SubscriptionPlanId);
            //userProfile.SubscriptionExpiryDate = DateTime.UtcNow.AddMonths(subscription.Months);
            //await this.userRepository.UpdateUserProfile(userProfile, CancellationToken.None);

            return new EmptyResult();
        }
    }
}
