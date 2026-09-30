using System;

namespace Clinic.Core.BLL.Doctors.DTOs
{
    public class DoctorScheduleDto
    {
        public int Id { get; set; }
        public byte DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
