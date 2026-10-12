using System;
using System.Collections.Generic;

namespace Clinic.Core.BLL.Patients.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int TotalItems { get => TotalCount; set => TotalCount = value; }
        public int PageNumber { get; set; }
        public int Page { get => PageNumber; set => PageNumber = value; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public PagedResult()
        {
            Items = new List<T>();
        }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    public class PatientHistoryListItemDto
    {
        public int Id { get; set; }
        public string PatientCode { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class PatientHistoryDetailsDto
    {
        public PatientDto Patient { get; set; }
        public List<VisitHistoryItemDto> Visits { get; set; }

        public PatientHistoryDetailsDto()
        {
            Visits = new List<VisitHistoryItemDto>();
        }
    }

    public class VisitHistoryItemDto
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public DateTime VisitDate { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string AppointmentStatus { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public string SpecialityNameAr { get; set; }
        public string SpecialityNameEn { get; set; }
        public string Notes { get; set; }
        public List<SymptomHistoryDto> Symptoms { get; set; }
        public List<DiagnosisHistoryDto> Diagnoses { get; set; }
        public List<PrescriptionHistoryDto> Prescriptions { get; set; }

        public VisitHistoryItemDto()
        {
            Symptoms = new List<SymptomHistoryDto>();
            Diagnoses = new List<DiagnosisHistoryDto>();
            Prescriptions = new List<PrescriptionHistoryDto>();
        }
    }

    public class SymptomHistoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }

    public class DiagnosisHistoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }

    public class PrescriptionHistoryDto
    {
        public int Id { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<PrescriptionItemHistoryDto> Items { get; set; }

        public PrescriptionHistoryDto()
        {
            Items = new List<PrescriptionItemHistoryDto>();
        }
    }

    public class PrescriptionItemHistoryDto
    {
        public int Id { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Notes { get; set; }
    }
}
