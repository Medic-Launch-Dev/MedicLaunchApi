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
            var newUser = CreateUser();
            newUser.UserName = user.Email;
            newUser.Email = user.Email;
            newUser.FirstName = user.FirstName;
            newUser.LastName = user.LastName;
            newUser.DisplayName = user.DisplayName;
            newUser.University = user.University;
            newUser.GraduationYear = user.GraduationYear;
            newUser.City = user.City;
            newUser.HowDidYouHearAboutUs = user.HowDidYouHearAboutUs;

            var result = await this.userManager.CreateAsync(newUser, user.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        private MedicLaunchUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<MedicLaunchUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(MedicLaunchUser)}'. " +
                    $"Ensure that '{nameof(MedicLaunchUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
