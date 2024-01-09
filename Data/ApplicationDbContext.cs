using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedicLaunchApi.Models;

namespace MedicLaunchApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<MedicLaunchUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {

        }
    }
}
