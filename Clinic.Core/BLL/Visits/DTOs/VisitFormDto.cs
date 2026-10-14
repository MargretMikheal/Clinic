using System;
using System.Collections.Generic;

namespace Clinic.Core.BLL.Visits.DTOs
{
    public class VisitFormDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string VisitNotes { get; set; }
        public List<int> SymptomIds { get; set; } = new List<int>();
        public List<int> DiagnosisIds { get; set; } = new List<int>();
        public string PrescriptionNotes { get; set; }
        public List<PrescriptionItemDto> PrescriptionItems { get; set; } = new List<PrescriptionItemDto>();
    }
}
