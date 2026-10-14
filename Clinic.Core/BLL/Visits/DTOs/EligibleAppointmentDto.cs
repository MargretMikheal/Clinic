using System;

namespace Clinic.Core.BLL.Visits.DTOs
{
    public class EligibleAppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientCode { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
