using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration
{
    public class AdministrationAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Administration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Administration_DoctorAppointments_Alias",
                "Administration/DoctorAppointments/{action}/{id}",
                new { controller = "DoctorAppointmentsReport", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "Administration_PatientMedicalHistory_Alias",
                "Administration/PatientMedicalHistory/{action}/{id}",
                new { controller = "PatientMedicalHistoryReport", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "Administration_DoctorPerformance_Alias",
                "Administration/DoctorPerformance/{action}/{id}",
                new { controller = "DoctorPerformanceReport", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "Administration_default",
                "Administration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
