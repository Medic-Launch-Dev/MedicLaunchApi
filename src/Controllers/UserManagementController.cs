using MedicLaunchApi.Authorization;
using MedicLaunchApi.Common;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Policy = RoleConstants.Admin)]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;
        private readonly PaymentService paymentService;

        public UserManagementController(UserManager<MedicLaunchUser> signInManager, PaymentService paymentService)
        {
            this.userManager = signInManager;
            this.paymentService = paymentService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserProfiles()
        {
            var userProfiles = await this.userManager.Users.ToListAsync();
            var adminUserProfiles = new List<UserProfileForAdmin>();
            foreach (var user in userProfiles)
            {
                var userRoles = await this.userManager.GetRolesAsync(user);
                adminUserProfiles.Add(new UserProfileForAdmin
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DisplayName = user.DisplayName,
                    Email = user.Email ?? string.Empty,
                    University = user.University,
                    GraduationYear = user.GraduationYear,
                    City = user.City ?? string.Empty,
                    SubscribeToPromotions = user.SubscribeToPromotions,
                    SubscriptionMonths = user.SubscriptionPlanId != null ? PaymentHelper.GetSubscriptionPlan(user.SubscriptionPlanId).Months.ToString() : "N/A",
                    SubscriptionPurchaseDate = user.SubscriptionCreatedDate.HasValue ? user.SubscriptionCreatedDate.Value.ToUniversalTime().ToString() : string.Empty,
                    UserRoles = userRoles,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    IsSubscribed = user.SubscriptionExpiryDate.HasValue && user.SubscriptionExpiryDate.Value > DateTime.UtcNow
                });
            }

            return Ok(adminUserProfiles);
        }

        [HttpPost("update")]
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
            user.PhoneNumber = userProfile.PhoneNumber;

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

        [HttpPost("resetuserpassword")]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetUserPasswordRequestForAdmin resetPasswordRequest)
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

        [HttpPost("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, [FromServices] PaymentService paymentService, [FromServices] IMixPanelService mixPanelService)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            await paymentService.DeleteStripeCustomer(user);
            await mixPanelService.DeleteUserProfile(user.Id);

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

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest user)
        {
            if (user == null || string.IsNullOrEmpty(user.SubscriptionPlanId))
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
                await this.paymentService.CreateStripeCustomerIfNotExists(newUser);
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}
