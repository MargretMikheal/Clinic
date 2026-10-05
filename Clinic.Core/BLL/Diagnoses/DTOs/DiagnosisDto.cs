using System;

namespace Clinic.Core.BLL.Diagnoses.DTOs
{
    public class DiagnosisDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
    }
}
