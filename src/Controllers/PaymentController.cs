using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Authorize]
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly PaymentService paymentService;

        public PaymentController(ILogger<PaymentController> logger, PaymentService paymentService)
        {
            this.logger = logger;
            this.paymentService = paymentService;
        }

        [HttpPost]
        [Route("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent(string planId)
        {
            try
            {
                var clientSecret = await this.paymentService.CreatePaymentIntent(planId, this.GetCurrentUserId());
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating payment intent");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("publishable-key")]
        public IActionResult GetPublishableKey()
        {
            return Ok(new { publishableKey = this.paymentService.GetPublishKey() });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
