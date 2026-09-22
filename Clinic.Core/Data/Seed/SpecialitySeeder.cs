using System;
using System.Linq;

namespace Clinic.Core.Data.Seed
{
    public static class SpecialitySeeder
    {
        public static void Seed(Core.Data.Entities context)
        {
            if (!context.Specialities.Any())
            {
                var now = DateTime.Now;

                context.Specialities.Add(new Core.Data.Speciality
                {
                    NameAr = "باطنة",
                    NameEn = "Internal Medicine",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Specialities.Add(new Core.Data.Speciality
                {
                    NameAr = "أطفال",
                    NameEn = "Pediatrics",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Specialities.Add(new Core.Data.Speciality
                {
                    NameAr = "جلدية",
                    NameEn = "Dermatology",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Specialities.Add(new Core.Data.Speciality
                {
                    NameAr = "قلب",
                    NameEn = "Cardiology",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Specialities.Add(new Core.Data.Speciality
                {
                    NameAr = "نساء وتوليد",
                    NameEn = "Obstetrics and Gynecology",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.SaveChanges();
            }
        }
    }
}