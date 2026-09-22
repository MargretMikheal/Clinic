using System;
using System.Collections.Generic;
using System.Linq;

namespace Clinic.Core.Data.Seed
{
    public static class ScreenSeeder
    {
        public static void Seed(Entities context)
        {
            // If table already contains data, do nothing
            if (context.Screens.Any())
                return;

            var screens = new List<Screen>
            {
                // =========================
                // Administration
                // =========================

                new Screen
                {
                    Name = "Administration",
                    Area = "Administration",
                    Icon = "fa-cogs",
                    ParentId = null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                // =========================
                // Reception
                // =========================

                new Screen
                {
                    Name = "Reception",
                    Area = "Reception",
                    Icon = "fa-desktop",
                    ParentId = null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                // =========================
                // Doctor
                // =========================

                new Screen
                {
                    Name = "Doctor",
                    Area = "Doctor",
                    Icon = "fa-user-md",
                    ParentId = null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                }
            };

            context.Screens.AddRange(screens);
            context.SaveChanges();

            // Get parent IDs
            var administrationId = context.Screens
                .First(x => x.Name == "Administration")
                .Id;

            var receptionId = context.Screens
                .First(x => x.Name == "Reception")
                .Id;

            var doctorId = context.Screens
                .First(x => x.Name == "Doctor")
                .Id;

            // =========================
            // Child Screen
            // =========================

            var childScreens = new List<Screen>
            {
                // Administration
                new Screen
                {
                    Name = "Users",
                    Area = "Administration",
                    Controller = "Users",
                    Action = "Index",
                    Icon = "fa-users",
                    ParentId = administrationId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Role Permissions",
                    Area = "Administration",
                    Controller = "RolePermissions",
                    Action = "Index",
                    Icon = "fa-lock",
                    ParentId = administrationId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Specialities",
                    Area = "Administration",
                    Controller = "Speciality",
                    Action = "Index",
                    Icon = "fa-stethoscope",
                    ParentId = administrationId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Symptoms",
                    Area = "Administration",
                    Controller = "Symptom",
                    Action = "Index",
                    Icon = "fa-list",
                    ParentId = administrationId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Diagnoses",
                    Area = "Administration",
                    Controller = "Diagnosis",
                    Action = "Index",
                    Icon = "fa-medkit",
                    ParentId = administrationId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                // Reception
                new Screen
                {
                    Name = "Patients",
                    Area = "Reception",
                    Controller = "Patient",
                    Action = "Index",
                    Icon = "fa-user",
                    ParentId = receptionId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Appointments",
                    Area = "Reception",
                    Controller = "Appointment",
                    Action = "Index",
                    Icon = "fa-calendar",
                    ParentId = receptionId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                // Doctor
                new Screen
                {
                    Name = "My Schedule",
                    Area = "Doctor",
                    Controller = "Schedule",
                    Action = "Index",
                    Icon = "fa-clock-o",
                    ParentId = doctorId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "My Appointments",
                    Area = "Doctor",
                    Controller = "Appointment",
                    Action = "Index",
                    Icon = "fa-calendar",
                    ParentId = doctorId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Patient History",
                    Area = "Doctor",
                    Controller = "Patient",
                    Action = "History",
                    Icon = "fa-history",
                    ParentId = doctorId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                },

                new Screen
                {
                    Name = "Visit",
                    Area = "Doctor",
                    Controller = "Visit",
                    Action = "Index",
                    Icon = "fa-stethoscope",
                    ParentId = doctorId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now
                }
            };

            context.Screens.AddRange(childScreens);

            context.SaveChanges();
        }
    }
}