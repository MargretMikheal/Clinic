using System;
using System.Linq;

namespace Clinic.Core.Data.Seed
{
    public static class SymptomSeeder
    {
        public static void Seed(Core.Data.Entities context)
        {
            if (!context.Symptoms.Any())
            {
                var now = DateTime.Now;

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "حمى",
                    NameEn = "Fever",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "صداع",
                    NameEn = "Headache",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "كحة",
                    NameEn = "Cough",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "ألم البطن",
                    NameEn = "Abdominal Pain",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "إرهاق",
                    NameEn = "Fatigue",
                    IsActive = true,
                    CreatedDate = now,
                    CreatedBy = "Seed",
                    IsDeleted = false
                });

                context.Symptoms.Add(new Core.Data.Symptom
                {
                    NameAr = "غثيان",
                    NameEn = "Nausea",
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