using System;

namespace Clinic.Core.BLL.Symptoms.DTOs
{
    public class SymptomDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
    }
}
