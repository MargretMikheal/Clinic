using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Clinic.Core.Identity.Stores;

namespace Clinic.Core.Data.Seed
{
    public static class RoleSeeder
    {
        public static void Seed(Clinic.Core.Data.Entities context)
        {
            if (!context.AspNetRoles.Any())
            {
                var store = new ApplicationRoleStore(context);
                var roleManager = new RoleManager<Clinic.Core.Identity.ApplicationRole>(store);

                roleManager.Create(new Clinic.Core.Identity.ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "Admin" });
                roleManager.Create(new Clinic.Core.Identity.ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "Doctor" });
                roleManager.Create(new Clinic.Core.Identity.ApplicationRole { Id = Guid.NewGuid().ToString(), Name = "Receptionist" });
            }
        }
    }
}
