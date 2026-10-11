using System;

namespace Clinic.Core.BLL.Appointments.DTOs
{
    public class AppointmentListDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientCode { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
