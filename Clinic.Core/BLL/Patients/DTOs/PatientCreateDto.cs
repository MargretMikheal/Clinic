using System;

namespace Clinic.Core.BLL.Patients.DTOs
{
    public class PatientCreateDto
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
