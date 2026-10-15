namespace Clinic.Core.BLL.Appointments.DTOs
{
    public class DoctorPerformanceDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public string SpecialityName { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int NoShowCount { get; set; }
    }
}
