using System;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class VisitDashboardItemDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime VisitDate { get; set; }
        public string Notes { get; set; }
    }
}
