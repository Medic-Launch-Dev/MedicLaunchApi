using Google.Apis.Auth;
using MedicLaunchApi.Models;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicLaunchApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<MedicLaunchUser> userManager;

        public AccountController(UserManager<MedicLaunchUser> signInManager)
        {
            this.userManager = signInManager;
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
                PasswordHash = user.Password
            };

            var result = await this.userManager.CreateAsync(newUser);
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
