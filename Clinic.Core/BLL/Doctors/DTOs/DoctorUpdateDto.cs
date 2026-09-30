using System.Collections.Generic;

namespace Clinic.Core.BLL.Doctors.DTOs
{
    public class DoctorUpdateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
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
