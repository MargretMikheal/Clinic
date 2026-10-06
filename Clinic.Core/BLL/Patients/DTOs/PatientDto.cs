using System;

namespace Clinic.Core.BLL.Patients.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
