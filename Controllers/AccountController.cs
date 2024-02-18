using Google.Apis.Auth;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicLaunchApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly PaymentService paymentService;
        private readonly UserRepository userRepository;
        public AccountController(UserManager<MedicLaunchUser> signInManager, PaymentService paymentService, UserRepository userRepository)
        {
            this.userManager = signInManager;
            this.paymentService = paymentService;
            this.userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest user)
        {
            var newUser = new MedicLaunchUser
            {
                UserName = user.Email,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                University = user.University,
                GraduationYear = user.GraduationYear,
                City = user.City,
                HowDidYouHearAboutUs = user.HowDidYouHearAboutUs,
                SubscribeToPromotions = user.SubscribeToPromotions
            };

            var result = await this.userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                // Create stripe customer to be used for future payments
                this.paymentService.CreateStripeCustomer(newUser);
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

    }
}
