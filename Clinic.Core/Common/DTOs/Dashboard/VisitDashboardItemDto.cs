using System;

namespace Clinic.Core.Common.DTOs.Dashboard
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
