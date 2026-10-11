using System;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.BLL.Appointments.DTOs
{
    public class UpdateAppointmentDto
    {
        [Required]
        public int Id { get; set; }
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
