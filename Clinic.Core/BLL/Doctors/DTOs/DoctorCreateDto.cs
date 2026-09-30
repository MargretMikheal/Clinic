using System.Collections.Generic;

namespace Clinic.Core.BLL.Doctors.DTOs
{
    public class DoctorCreateDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DoctorCode { get; set; }
        public string Name { get; set; }
        public int SpecialityId { get; set; }
        public string Phone { get; set; }
        public decimal ExaminationPrice { get; set; }
        public int SlotDuration { get; set; }
        public bool IsActive { get; set; }
        public string ReceptionistUserId { get; set; }
        public List<DoctorScheduleDto> Schedules { get; set; }
    }
}
