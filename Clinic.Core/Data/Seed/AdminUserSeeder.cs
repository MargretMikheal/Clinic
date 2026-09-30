using Microsoft.AspNet.Identity;
using System.Linq;
using Clinic.Core.Identity.Stores;

namespace Clinic.Core.Data.Seed
{
    public static class AdminUserSeeder
    {
        public static void Seed(Clinic.Core.Data.Entities context)
        {
            if (context.AspNetUsers.Any())
                return;

            var store = new ApplicationUserStore(context);
            var userManager = new UserManager<Clinic.Core.Identity.ApplicationUser>(store);
            
            // Note: ASP.NET Identity password hashing needs PasswordHasher when creating passwords,
            // UserManager automatically uses its internal IPasswordHasher when we call Create() with a password.

            var admin = new Clinic.Core.Identity.ApplicationUser
            {
                Id = System.Guid.NewGuid().ToString(),
                UserName = "admin",
                Email = "admin@clinic.com",
                EmailConfirmed = true
            };

            var result = userManager.Create(admin, "Admin@123");

            if (!result.Succeeded)
            {
                throw new System.Exception(
                    "Failed to create Admin user: " +
                    string.Join(", ", result.Errors));
            }

            var roleResult = userManager.AddToRole(admin.Id, Clinic.Core.Enums.UserRole.Admin.ToString());

            if (!roleResult.Succeeded)
            {
                throw new System.Exception(
                    "Failed to assign Admin role: " +
                    string.Join(", ", roleResult.Errors));
            }
        }
    }
}