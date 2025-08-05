using MedicLaunchApi.Authorization;
using MedicLaunchApi.Common;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicLaunchApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly QuestionRepository questionRepository;
        private readonly PaymentService paymentService;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<MedicLaunchUser> signInManager, QuestionRepository questionRepository, PaymentService paymentService, IConfiguration configuration)
        {
            this.userManager = signInManager;
            this.questionRepository = questionRepository;
            this.paymentService = paymentService;
            this.configuration = configuration;
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
                SubscribeToPromotions = user.SubscribeToPromotions,
                PhoneNumber = user.PhoneNumber,
                CreatedOn = DateTime.UtcNow
            };

            var result = await this.userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                await this.userManager.AddToRoleAsync(newUser, RoleConstants.Student);
                await this.paymentService.CreateStripeCustomerIfNotExists(newUser);
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("Invalid email confirmation token");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                var domain = configuration.GetValue<string>("ReactApp:Url");
                return Redirect(domain + "/email-confirmed");
            }

            return BadRequest("Error confirming your email.");
        }

        [HttpGet("myprofile")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var subscriptionPlan = user.SubscriptionPlanId != null ?
                PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId) : null;

            const int TrialQuestionLimit = 150;
            const int TrialClinicalCaseLimit = 5;

            var userProfile = new MyUserProfile
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                University = user.University,
                GraduationYear = user.GraduationYear,
                City = user.City ?? string.Empty,
                SubscribeToPromotions = user.SubscribeToPromotions,
                SubscriptionMonths = subscriptionPlan != null ? subscriptionPlan.Months.ToString() : "N/A",
                SubscriptionPurchaseDate = user.SubscriptionCreatedDate.HasValue ? user.SubscriptionCreatedDate.Value.ToUniversalTime().ToString() : string.Empty,
                QuestionsCompleted = questionRepository.GetTotalAttemptedQuestionsForUser(user.Id),
                HasActiveSubscription = user.HasActiveSubscription,
                IsOnFreeTrial = user.IsOnFreeTrial,
                FreeTrialDaysRemaining = user.FreeTrialDaysRemaining,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                RemainingTrialQuestions = TrialQuestionLimit - user.TrialQuestionsAttemptedCount,
                RemainingTrialClinicalCases = TrialClinicalCaseLimit - user.TrialClinicalCasesGeneratedCount,
                StripeSubscriptionStatus = user.StripeSubscriptionStatus
            };

            return Ok(userProfile);
        }

        [HttpPut("edit")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] RegisterUserRequest user)
        {
            var currentUser = await this.userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            currentUser.FirstName = user.FirstName;
            currentUser.LastName = user.LastName;
            currentUser.DisplayName = user.DisplayName;
            currentUser.University = user.University;
            currentUser.GraduationYear = user.GraduationYear;
            currentUser.City = user.City;
            currentUser.PhoneNumber = user.PhoneNumber;

            var result = await this.userManager.UpdateAsync(currentUser);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpGet("roles")]
        [Authorize]
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

        [HttpGet("hasactivesubscription")]
        [Authorize]
        public async Task<IActionResult> HasActiveSubscription()
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // If user is Admin or QuestionAuthor, they have access to all questions
            if (User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.QuestionAuthor))
            {
                return Ok(true);
            }

            return Ok(user.SubscriptionExpiryDate.HasValue && user.SubscriptionExpiryDate.Value > DateTime.UtcNow);
        }

        [HttpPost("resetpassword")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestForStudent resetPasswordRequest)
        {
            var user = await this.userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await this.userManager.ChangePasswordAsync(user, resetPasswordRequest.CurrentPassword, resetPasswordRequest.NewPassword);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}
