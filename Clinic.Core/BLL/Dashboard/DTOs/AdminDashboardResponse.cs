using System.Collections.Generic;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class AdminDashboardResponse
    {
        public List<DashboardStatisticDto> Statistics { get; set; }

        public List<DoctorDashboardItemDto> Doctors { get; set; }

        public List<PatientDashboardItemDto> Patients { get; set; }

        public List<SpecialityDashboardItemDto> Specialities { get; set; }

        public List<AppointmentDashboardItemDto> TodayAppointments { get; set; }

        public List<VisitDashboardItemDto> TodayCompletedVisits { get; set; }

        public List<AppointmentDashboardItemDto> TodayCancelledAppointments { get; set; }

        public AdminDashboardResponse()
        {
            Statistics = new List<DashboardStatisticDto>();
            Doctors = new List<DoctorDashboardItemDto>();
            Patients = new List<PatientDashboardItemDto>();
            Specialities = new List<SpecialityDashboardItemDto>();
            TodayAppointments = new List<AppointmentDashboardItemDto>();
            TodayCompletedVisits = new List<VisitDashboardItemDto>();
            TodayCancelledAppointments = new List<AppointmentDashboardItemDto>();
        }
    }
}
