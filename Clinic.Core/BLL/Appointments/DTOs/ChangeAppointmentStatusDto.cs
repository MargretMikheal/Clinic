using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.BLL.Appointments.DTOs
{
    public class ChangeAppointmentStatusDto
    {
        [Required]
        public int AppointmentId { get; set; }
        
        [Required]
        public string Status { get; set; }
    }
}
