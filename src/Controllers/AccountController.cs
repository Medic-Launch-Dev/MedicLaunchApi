using MedicLaunchApi.Authorization;
using MedicLaunchApi.Common;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicLaunchApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly PaymentService paymentService;
        private readonly QuestionRepository questionRepository;
		private readonly IEmailSender emailSender;
		private readonly IConfiguration configuration;
        private readonly ILogger<AccountController> logger;
		public AccountController(
            UserManager<MedicLaunchUser> userManager,
			PaymentService paymentService,
			QuestionRepository questionRepository,
			IEmailSender emailSender,
			IConfiguration configuration,
			ILogger<AccountController> logger)
		{
			this.userManager = userManager;

			this.paymentService = paymentService;
			this.questionRepository = questionRepository;
			this.emailSender = emailSender;
			this.configuration = configuration;
			this.logger = logger;
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
                PhoneNumber = user.PhoneNumber
            };

            var result = await this.userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                // Create stripe customer to be used for future payments
                this.paymentService.CreateStripeCustomer(newUser);

                // Default role is student - use role manager to add roles
                await this.userManager.AddToRoleAsync(newUser, RoleConstants.Student);

				var token = await this.userManager.GenerateEmailConfirmationTokenAsync(newUser);
				var domain = configuration.GetValue<string>("ReactApp:Url");
                var confirmationLink = $"{domain}/confirm-email?userId={newUser.Id}&token={token}";
				var message = $"Please confirm your email by clicking this link: {confirmationLink}";
				await this.emailSender.SendEmailAsync(newUser.Email, "Confirm Your Email", message);

				return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

		[HttpPost("resend-confirmation-email")]
		public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
		{
			var user = await this.userManager.FindByEmailAsync(request.Email);
			if (user == null)
			{
				return NotFound(new { Message = "User not found." });
			}

			if (await this.userManager.IsEmailConfirmedAsync(user))
			{
				return BadRequest(new { Message = "Email is already confirmed." });
			}

			var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            this.logger.LogInformation(token);
			var domain = configuration.GetValue<string>("ReactApp:Url");
			var confirmationLink = $"{domain}/confirm-email?userId={user.Id}&token={token}";
			var message = $"Please confirm your email by clicking this link: {confirmationLink}";

			await this.emailSender.SendEmailAsync(user.Email, "Confirm Your Email", message);

			return Ok();
		}

		[HttpGet("confirm-email")]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
			{
				return BadRequest("User ID and token are required.");
			}

			var user = await this.userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

            this.logger.LogInformation(token);

			var result = await this.userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				return Ok("Email confirmed successfully.");
			}
			else
			{
				return BadRequest(result.Errors);
			}
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

            var subscriptionPlan = user.SubscriptionPlanId != null ? PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId) : null;
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
                HasActiveSubscription = user.SubscriptionExpiryDate.HasValue && user.SubscriptionExpiryDate.Value > DateTime.UtcNow,
                PhoneNumber = user.PhoneNumber ?? string.Empty
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
