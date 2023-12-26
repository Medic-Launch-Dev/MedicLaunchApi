using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace MedicLaunchApi.Repository
{
    public abstract class UserRepositoryBase
    {
        protected readonly HttpContextAccessor httpContextAccessor;

        public UserRepositoryBase(HttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected virtual string GetUserBlobPath()
        {
            var claimsIdentity = (ClaimsIdentity)this.httpContextAccessor.HttpContext.User.Identity;
            var userId = claimsIdentity.Claims.FirstOrDefault(claim => claim.Type == ClaimConstants.Oid);
            return $"{userId}";
        }
    }
}
