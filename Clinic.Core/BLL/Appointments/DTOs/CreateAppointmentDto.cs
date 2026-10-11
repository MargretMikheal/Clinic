using System;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.BLL.Appointments.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public int PatientId { get; set; }
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        public string Notes { get; set; }
    }
}
