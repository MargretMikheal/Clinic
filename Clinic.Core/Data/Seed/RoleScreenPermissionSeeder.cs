using System;
using System.Collections.Generic;
using System.Linq;

namespace Clinic.Core.Data.Seed
{
    public static class RoleScreenPermissionSeeder
    {
        public static void Seed(Core.Data.Entities context)
        {
            // If table already contains data, do nothing
            if (context.RoleScreenPermissions.Any())
                return;

            var adminRole = context.AspNetRoles
                .FirstOrDefault(x => x.Name == "Admin");

            var doctorRole = context.AspNetRoles
                .FirstOrDefault(x => x.Name == "Doctor");

            var receptionistRole = context.AspNetRoles
                .FirstOrDefault(x => x.Name == "Receptionist");

            if (adminRole == null ||
                doctorRole == null ||
                receptionistRole == null)
            {
                throw new Exception(
                    "Required roles were not found. Seed Roles before RoleScreenPermission."
                );
            }

            var screens = context.Screens
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToList();

            var permissions = new List<RoleScreenPermission>();

            // =====================================================
            // ADMIN - FULL ACCESS
            // =====================================================

            foreach (var screen in screens)
            {
                permissions.Add(new RoleScreenPermission
                {
                    RoleId = adminRole.Id,
                    ScreenId = screen.Id,

                    ScreenShow = true,
                    ScreenAdd = true,
                    ScreenEdit = true,
                    ScreenDelete = true,
                    ScreenPrint = true,

                    CreatedDate = DateTime.Now
                });
            }

            // =====================================================
            // RECEPTIONIST
            // =====================================================

            var receptionistScreenNames = new[]
            {
                "Reception",
                "Patients",
                "Appointments"
            };

            foreach (var screen in screens
                .Where(x => receptionistScreenNames.Contains(x.Name)))
            {
                permissions.Add(new RoleScreenPermission
                {
                    RoleId = receptionistRole.Id,
                    ScreenId = screen.Id,

                    ScreenShow = true,
                    ScreenAdd = true,
                    ScreenEdit = true,
                    ScreenDelete = false,
                    ScreenPrint = true,

                    CreatedDate = DateTime.Now
                });
            }

            // =====================================================
            // DOCTOR
            // =====================================================

            var doctorScreenNames = new[]
            {
                "Doctor",
                "My Schedule",
                "My Appointments",
                "Patient History",
                "Visit"
            };

            foreach (var screen in screens
                .Where(x => doctorScreenNames.Contains(x.Name)))
            {
                permissions.Add(new RoleScreenPermission
                {
                    RoleId = doctorRole.Id,
                    ScreenId = screen.Id,

                    ScreenShow = true,

                    ScreenAdd =
                        screen.Name == "My Schedule" ||
                        screen.Name == "Visit",

                    ScreenEdit =
                        screen.Name == "My Schedule" ||
                        screen.Name == "Visit",

                    ScreenDelete = false,

                    ScreenPrint =
                        screen.Name == "Patient History" ||
                        screen.Name == "Visit",

                    CreatedDate = DateTime.Now
                });
            }

            context.RoleScreenPermissions.AddRange(permissions);

            context.SaveChanges();
        }
    }
}