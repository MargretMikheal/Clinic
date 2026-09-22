using System;
using System.Linq;

namespace Clinic.Core.Data.Seed
{
    public static class DiagnosisSeeder
    {
        public static void Seed(Core.Data.Entities context)
        {
            if (!context.Diagnosis.Any())
            {
                var now = DateTime.Now;

                context.Diagnosis.Add(new Core.Data.Diagnosi
                {
                    NameAr = "نزلة برد",
                    NameEn = "Common Cold",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Diagnosis.Add(new Core.Data.Diagnosi
                {
                    NameAr = "إنفلونزا",
                    NameEn = "Influenza",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Diagnosis.Add(new Core.Data.Diagnosi
                {
                    NameAr = "صداع نصفي",
                    NameEn = "Migraine",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Diagnosis.Add(new Core.Data.Diagnosi
                {
                    NameAr = "التهاب المعدة",
                    NameEn = "Gastritis",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Diagnosis.Add(new Core.Data.Diagnosi
                {
                    NameAr = "التهاب الحلق",
                    NameEn = "Pharyngitis",
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