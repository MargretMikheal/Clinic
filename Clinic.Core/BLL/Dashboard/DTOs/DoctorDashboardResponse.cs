using System.Collections.Generic;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class DoctorDashboardResponse
    {
        public List<DashboardStatisticDto> Statistics { get; set; }

        public List<ScheduleDashboardItemDto> TodaySchedule { get; set; }

        public List<PatientDashboardItemDto> RecentPatients { get; set; }
        
        public List<AppointmentDashboardItemDto> TodayAppointments { get; set; }

        public List<VisitDashboardItemDto> CompletedAppointments { get; set; }

        public List<AppointmentDashboardItemDto> RemainingAppointments { get; set; }

        public DoctorDashboardResponse()
        {
            Statistics = new List<DashboardStatisticDto>();
            TodaySchedule = new List<ScheduleDashboardItemDto>();
            RecentPatients = new List<PatientDashboardItemDto>();
            TodayAppointments = new List<AppointmentDashboardItemDto>();
            CompletedAppointments = new List<VisitDashboardItemDto>();
            RemainingAppointments = new List<AppointmentDashboardItemDto>();
        }
    }
}
