using System;

namespace Clinic.Core.BLL.Specialities.DTOs
{
    public class SpecialityDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public bool IsActive { get; set; }
    }
}
