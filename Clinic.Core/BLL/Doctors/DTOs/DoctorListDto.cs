namespace Clinic.Core.BLL.Doctors.DTOs
{
    public class DoctorListDto
    {
        public int Id { get; set; }
        public string DoctorCode { get; set; }
        public string Name { get; set; }
        public string SpecialityName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal ExaminationPrice { get; set; }
        public int SlotDuration { get; set; }
        public string ReceptionistName { get; set; }
        public bool IsActive { get; set; }
    }
}
