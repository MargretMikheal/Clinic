using System.Collections.Generic;

namespace Clinic.Core.BLL.Doctors.DTOs
{
    public class DoctorWeeklyScheduleDto
    {
        public int DoctorId { get; set; }
        public string DoctorCode { get; set; }
        public string DoctorName { get; set; }
        public int SlotDuration { get; set; }
        public List<DoctorScheduleDto> Schedules { get; set; }

        public DoctorWeeklyScheduleDto()
        {
            Schedules = new List<DoctorScheduleDto>();
        }
    }
}
