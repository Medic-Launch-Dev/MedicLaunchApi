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
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly PaymentService paymentService;
        private readonly UserRepository userRepository;
        private readonly QuestionRepository questionRepository;

        public UserManagementController(UserManager<MedicLaunchUser> signInManager, PaymentService paymentService, UserRepository userRepository, QuestionRepository questionRepository)
        {
            this.userManager = signInManager;
            this.paymentService = paymentService;
            this.userRepository = userRepository;
            this.questionRepository = questionRepository;
        }

        [HttpGet("list")]
        // TODO: make sure only admin (i.e. sajjaad can do this)
        public async Task<IActionResult> GetUserProfiles()
        {
            var users = this.userManager.Users.ToList();
            var userProfiles = new List<UserProfileModel>();
            var tasks = users.Select(async user =>
            {
                var subscriptionPlan = user.SubscriptionPlanId != null ? PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId) : null;
                var userProfile = new UserProfileModel
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email ?? string.Empty,
                    University = user.University,
                    GraduationYear = user.GraduationYear,
                    City = user.City ?? string.Empty,
                    SubscribeToPromotions = user.SubscribeToPromotions,
                    SubscriptionMonths = subscriptionPlan != null ? subscriptionPlan.Months.ToString() : "N/A",
                    SubscriptionPurchaseDate = user.SubscriptionCreatedDate != null ? user.SubscriptionCreatedDate.Value.ToString("yyyy-MM-dd hh:mm tt") : "N/A",
                    QuestionsCompleted = await GetQuestionsCompleted(user.Id)
                };
                userProfiles.Add(userProfile);
            });

            await Task.WhenAll(tasks);

            return Ok(userProfiles);
        }

        [HttpGet("update")]
        // TODO: make sure only admin (i.e. sajjaad can do this)
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserRequest userProfile)
        {
            var user = await this.userManager.FindByIdAsync(userProfile.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = userProfile.FirstName;
            user.LastName = userProfile.LastName;
            user.DisplayName = userProfile.DisplayName;
            user.University = userProfile.University;
            user.GraduationYear = userProfile.GraduationYear;
            user.City = userProfile.City;

            var result = await this.userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("resetpassword")]
        // TODO: make sure only admin (i.e. sajjaad can do this)
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var user = await this.userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
            var result = await this.userManager.ResetPasswordAsync(user, token, resetPasswordRequest.NewPassword);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("delete")]
        // TODO: make sure only admin (i.e. sajjaad can do this)
        public async Task<IActionResult> DeleteUser([FromBody] string userId)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await this.userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        // add endpoint to add a user along with a subscription plan. Reuse the RegisterUserRequest model
        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest user)
        {
            if(user == null || string.IsNullOrEmpty(user.SubscriptionPlanId))
            {
                return BadRequest("Subscription plan is required");
            }

            var subscriptionPlan = PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId);

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
                SubscriptionPlanId = user.SubscriptionPlanId,
                SubscriptionCreatedDate = DateTime.UtcNow,
                SubscriptionExpiryDate = DateTime.UtcNow.AddMonths(subscriptionPlan.Months)
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

        private async Task<int> GetQuestionsCompleted(string userId)
        {
            var attemptedQuestions = await this.questionRepository.GetAttemptedQuestionsAsync(userId);
            return attemptedQuestions.DistinctBy(a => a.QuestionId).Count();
        }
    }
}
