using System;

namespace Clinic.Core.BLL.Visits.DTOs
{
    public class PrescriptionItemDto
    {
        public int? Id { get; set; }
        public int? PrescriptionId { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Notes { get; set; }
    }
}
