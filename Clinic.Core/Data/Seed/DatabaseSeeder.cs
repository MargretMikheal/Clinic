namespace Clinic.Core.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static void Seed()
        {
            // Business Database
            using (var clinicContext =
                new Core.Data.Entities())
            {
                SpecialitySeeder.Seed(clinicContext);
                SymptomSeeder.Seed(clinicContext);
                DiagnosisSeeder.Seed(clinicContext);
                ScreenSeeder.Seed(clinicContext);
                RoleScreenPermissionSeeder.Seed(clinicContext);
                RoleSeeder.Seed(clinicContext);
                AdminUserSeeder.Seed(clinicContext);
            }
        }
    }
}