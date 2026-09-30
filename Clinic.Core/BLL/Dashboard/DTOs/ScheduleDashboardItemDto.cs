using System;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class ScheduleDashboardItemDto
    {
        public int Id { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
