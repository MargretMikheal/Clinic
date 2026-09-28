using System;

namespace Clinic.Core.Common.DTOs.Dashboard
{
    public class AppointmentDashboardItemDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
    }
}
