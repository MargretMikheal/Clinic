using System.Collections.Generic;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class ReceptionistDashboardResponse
    {
        public List<DashboardStatisticDto> Statistics { get; set; }

        public string DoctorName { get; set; }
        
        public List<AppointmentDashboardItemDto> TodayAppointments { get; set; }

        public List<AppointmentDashboardItemDto> ConfirmedAppointments { get; set; }

        public List<VisitDashboardItemDto> CompletedAppointments { get; set; }

        public List<AppointmentDashboardItemDto> RemainingAppointments { get; set; }

        public ReceptionistDashboardResponse()
        {
            Statistics = new List<DashboardStatisticDto>();
            TodayAppointments = new List<AppointmentDashboardItemDto>();
            ConfirmedAppointments = new List<AppointmentDashboardItemDto>();
            CompletedAppointments = new List<VisitDashboardItemDto>();
            RemainingAppointments = new List<AppointmentDashboardItemDto>();
        }
    }
}
