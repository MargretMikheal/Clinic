namespace Clinic.Core.BLL.Visits.DTOs
{
    public class PrescriptionReportItemDto
    {
        public int Id { get; set; }
        public string PrescriptionCode { get; set; }
        public string VisitDate { get; set; }
        public string PatientName { get; set; }
        public string PatientCode { get; set; }
        public string PatientPhone { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public int DoctorId { get; set; }
        public int ItemCount { get; set; }
    }
}
