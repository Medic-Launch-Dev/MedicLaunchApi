using Microsoft.AspNetCore.Identity;

namespace MedicLaunchApi.Repository
{
    public abstract class UserRepositoryBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public UserRepositoryBase(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }
    }
}
