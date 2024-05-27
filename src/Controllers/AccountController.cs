using Google.Apis.Auth;
using MedicLaunchApi.Authorization;
using MedicLaunchApi.Common;
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
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly PaymentService paymentService;
        private readonly UserDataRepository userRepository;
        private readonly QuestionRepository questionRepository;
        public AccountController(UserManager<MedicLaunchUser> signInManager, PaymentService paymentService, UserDataRepository userRepository, RoleManager<IdentityRole> roleManager, QuestionRepository questionRepository)
        {
            this.userManager = signInManager;
            this.paymentService = paymentService;
            this.userRepository = userRepository;
            this.roleManager = roleManager;
            this.questionRepository = questionRepository;
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

                // Default role is student - use role manager to add roles
                await this.userManager.AddToRoleAsync(newUser, RoleConstants.Student);

                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("myprofile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var subscriptionPlan = user.SubscriptionPlanId != null ? PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId) : null;
            var userProfile = new MyUserProfile
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                University = user.University,
                GraduationYear = user.GraduationYear,
                City = user.City ?? string.Empty,
                SubscribeToPromotions = user.SubscribeToPromotions,
                SubscriptionMonths = subscriptionPlan != null ? subscriptionPlan.Months.ToString() : "N/A",
                SubscriptionPurchaseDate = user.SubscriptionCreatedDate.HasValue ? user.SubscriptionCreatedDate.Value.ToUniversalTime().ToString() : string.Empty,
                QuestionsCompleted = questionRepository.GetTotalAttemptedQuestionsForUser(user.Id)
            };

            return Ok(userProfile);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await this.userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }
}
