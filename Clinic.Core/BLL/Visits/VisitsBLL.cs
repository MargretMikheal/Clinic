using Clinic.Core.BLL.Visits.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class VisitsBLL
    {
        private readonly Entities _context;

        public VisitsBLL() : this(new Entities())
        {
        }

        public VisitsBLL(Entities context)
        {
            _context = context ?? new Entities();
        }

        public async Task<BaseResponse<List<EligibleAppointmentDto>>> GetTodayEligibleAppointmentsAsync(string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return BaseResponse<List<EligibleAppointmentDto>>.Fail("Unauthorized access.");
                }

                _context.usp_Appointment_MarkExpiredAsNoShow();

                var doctor = await _context.Doctors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                if (doctor == null || !doctor.IsActive)
                {
                    return BaseResponse<List<EligibleAppointmentDto>>.Fail("Doctor not found or inactive.");
                }

                var today = DateTime.Today;
                var bookedStatus = AppointmentStatus.Booked.ToString();
                var confirmedStatus = AppointmentStatus.Confirmed.ToString();

                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id &&
                                a.AppointmentDate == today &&
                                !a.IsDeleted &&
                                (a.Status == bookedStatus || a.Status == confirmedStatus) &&
                                !a.Visits.Any(v => !v.IsDeleted))
                    .OrderBy(a => a.AppointmentTime)
                    .Select(a => new EligibleAppointmentDto
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        PatientCode = a.Patient.PatientCode,
                        PatientName = a.Patient.Name,
                        PatientPhone = a.Patient.Phone,
                        Gender = a.Patient.Gender,
                        BirthDate = a.Patient.BirthDate,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status,
                        Notes = a.Notes
                    })
                    .ToListAsync();

                return BaseResponse<List<EligibleAppointmentDto>>.Success(appointments);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<EligibleAppointmentDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<SaveVisitResultDto>> SaveVisitAsync(VisitFormDto dto, string currentUserId)
        {
            try
            {
                if (dto == null)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("Invalid request data.");
                }

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("Unauthorized access.");
                }

                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                if (doctor == null || !doctor.IsActive)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("Doctor not found or inactive.");
                }

                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && !a.IsDeleted);

                if (appointment == null)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("Appointment not found.");
                }

                if (appointment.DoctorId != doctor.Id)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("You are not authorized to conduct a visit for this appointment.");
                }

                var today = DateTime.Today;
                if (appointment.AppointmentDate.Date != today)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("Only today's appointments can be processed for visit.");
                }

                var bookedStatus = AppointmentStatus.Booked.ToString();
                var confirmedStatus = AppointmentStatus.Confirmed.ToString();

                if (appointment.Status != bookedStatus && appointment.Status != confirmedStatus)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("The appointment is not in an eligible status (must be Booked or Confirmed).");
                }

                bool hasExistingVisit = await _context.Visits
                    .AnyAsync(v => v.AppointmentId == appointment.Id && !v.IsDeleted);

                if (hasExistingVisit)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("A visit has already been recorded for this appointment.");
                }

                if (dto.SymptomIds == null || !dto.SymptomIds.Any())
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("At least one symptom must be selected.");
                }

                var distinctSymptomIds = dto.SymptomIds.Distinct().ToList();
                var activeSymptomCount = await _context.Symptoms
                    .CountAsync(s => distinctSymptomIds.Contains(s.Id) && !s.IsDeleted && s.IsActive);

                if (activeSymptomCount != distinctSymptomIds.Count)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("One or more selected symptoms are invalid or inactive.");
                }

                if (dto.DiagnosisIds == null || !dto.DiagnosisIds.Any())
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("At least one diagnosis must be selected.");
                }

                var distinctDiagnosisIds = dto.DiagnosisIds.Distinct().ToList();
                var activeDiagnosisCount = await _context.Diagnosis
                    .CountAsync(d => distinctDiagnosisIds.Contains(d.Id) && !d.IsDeleted && d.IsActive);

                if (activeDiagnosisCount != distinctDiagnosisIds.Count)
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("One or more selected diagnoses are invalid or inactive.");
                }

                if (dto.PrescriptionItems == null || !dto.PrescriptionItems.Any())
                {
                    return BaseResponse<SaveVisitResultDto>.Fail("At least one prescription item is required.");
                }

                foreach (var item in dto.PrescriptionItems)
                {
                    if (string.IsNullOrWhiteSpace(item.MedicineName) ||
                        string.IsNullOrWhiteSpace(item.Dosage) ||
                        string.IsNullOrWhiteSpace(item.Frequency) ||
                        string.IsNullOrWhiteSpace(item.Duration))
                    {
                        return BaseResponse<SaveVisitResultDto>.Fail("Medicine name, dosage, frequency, and duration are required for all prescription items.");
                    }
                }

                // 6. Execute Transaction
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;

                        // Create Visit
                        var visit = new Visit
                        {
                            AppointmentId = appointment.Id,
                            PatientId = appointment.PatientId,
                            DoctorId = doctor.Id,
                            VisitDate = now,
                            Notes = dto.VisitNotes?.Trim(),
                            CreatedDate = now,
                            CreatedBy = currentUserId,
                            IsDeleted = false
                        };

                        _context.Visits.Add(visit);
                        await _context.SaveChangesAsync();

                        // Create VisitSymptoms
                        foreach (var symptomId in distinctSymptomIds)
                        {
                            _context.VisitSymptoms.Add(new VisitSymptom
                            {
                                VisitId = visit.Id,
                                SymptomId = symptomId
                            });
                        }

                        // Create VisitDiagnosi
                        foreach (var diagId in distinctDiagnosisIds)
                        {
                            _context.VisitDiagnosis.Add(new VisitDiagnosi
                            {
                                VisitId = visit.Id,
                                DiagnosisId = diagId
                            });
                        }

                        // Create Prescription
                        var prescription = new Prescription
                        {
                            VisitId = visit.Id,
                            Notes = dto.PrescriptionNotes?.Trim(),
                            CreatedDate = now,
                            CreatedBy = currentUserId,
                            IsDeleted = false
                        };

                        _context.Prescriptions.Add(prescription);
                        await _context.SaveChangesAsync();

                        // Create PrescriptionItems
                        foreach (var item in dto.PrescriptionItems)
                        {
                            _context.PrescriptionItems.Add(new PrescriptionItem
                            {
                                PrescriptionId = prescription.Id,
                                MedicineName = item.MedicineName.Trim(),
                                Dosage = item.Dosage.Trim(),
                                Frequency = item.Frequency.Trim(),
                                Duration = item.Duration.Trim(),
                                Notes = item.Notes?.Trim(),
                                CreatedDate = now,
                                CreatedBy = currentUserId,
                                IsDeleted = false
                            });
                        }

                        // Complete Appointment
                        appointment.Status = AppointmentStatus.Completed.ToString();
                        appointment.ModifiedDate = now;
                        appointment.ModifiedBy = currentUserId;

                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        return BaseResponse<SaveVisitResultDto>.Success(new SaveVisitResultDto
                        {
                            VisitId = visit.Id,
                            PrescriptionId = prescription.Id,
                            AppointmentId = appointment.Id
                        }, "Visit saved successfully.");
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return BaseResponse<SaveVisitResultDto>.Fail("An error occurred while saving the visit.");
                    }
                }
            }
            catch (Exception)
            {
                return BaseResponse<SaveVisitResultDto>.Fail("An unexpected error occurred.");
            }
        }

        public async Task<BaseResponse<List<PrescriptionReportItemDto>>> GetPrescriptionsForReportAsync(
            string currentUserId, bool isAdmin, string term = null, int? prescriptionId = null)
        {
            try
            {
                var query = _context.Prescriptions
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && !p.Visit.IsDeleted);

                if (!isAdmin)
                {
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        return BaseResponse<List<PrescriptionReportItemDto>>.Fail("Unauthorized access.");
                    }
                    var doctor = await _context.Doctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                    if (doctor == null)
                    {
                        return BaseResponse<List<PrescriptionReportItemDto>>.Success(new List<PrescriptionReportItemDto>());
                    }

                    query = query.Where(p => p.Visit.DoctorId == doctor.Id);
                }

                if (prescriptionId.HasValue && prescriptionId.Value > 0)
                {
                    query = query.Where(p => p.Id == prescriptionId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.Trim();
                    int parsedId;
                    if (int.TryParse(term, out parsedId))
                    {
                        query = query.Where(p => p.Id == parsedId ||
                                                 p.Visit.Patient.Name.Contains(term) ||
                                                 p.Visit.Patient.PatientCode.Contains(term) ||
                                                 p.Visit.Patient.Phone.Contains(term) ||
                                                 p.Visit.Doctor.Name.Contains(term));
                    }
                    else
                    {
                        query = query.Where(p => p.Visit.Patient.Name.Contains(term) ||
                                                 p.Visit.Patient.PatientCode.Contains(term) ||
                                                 p.Visit.Patient.Phone.Contains(term) ||
                                                 p.Visit.Doctor.Name.Contains(term));
                    }
                }

                var list = await query
                    .OrderByDescending(p => p.Id)
                    .Take(50)
                    .Select(p => new
                    {
                        p.Id,
                        VisitDate = p.Visit.VisitDate,
                        PatientName = p.Visit.Patient.Name,
                        PatientCode = p.Visit.Patient.PatientCode,
                        PatientPhone = p.Visit.Patient.Phone,
                        DoctorName = p.Visit.Doctor.Name,
                        DoctorCode = p.Visit.Doctor.DoctorCode,
                        DoctorId = p.Visit.DoctorId,
                        ItemCount = p.PrescriptionItems.Count(i => !i.IsDeleted)
                    })
                    .ToListAsync();

                var result = list.Select(p => new PrescriptionReportItemDto
                {
                    Id = p.Id,
                    PrescriptionCode = "Rx #" + p.Id,
                    VisitDate = p.VisitDate.ToString("yyyy-MM-dd"),
                    PatientName = p.PatientName,
                    PatientCode = p.PatientCode,
                    PatientPhone = p.PatientPhone,
                    DoctorName = p.DoctorName,
                    DoctorCode = p.DoctorCode,
                    DoctorId = p.DoctorId,
                    ItemCount = p.ItemCount
                }).ToList();

                return BaseResponse<List<PrescriptionReportItemDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<PrescriptionReportItemDto>>.Fail(ex.Message);
            }
        }

        public bool ValidatePrescriptionAccess(string currentUserId, bool isAdmin, int prescriptionId)
        {
            if (isAdmin)
            {
                return _context.Prescriptions.Any(p => p.Id == prescriptionId && !p.IsDeleted && !p.Visit.IsDeleted);
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return false;
            }

            return _context.Prescriptions
                .Any(p => p.Id == prescriptionId &&
                          !p.IsDeleted &&
                          !p.Visit.IsDeleted &&
                          p.Visit.Doctor.UserId == currentUserId &&
                          !p.Visit.Doctor.IsDeleted);
        }
    }
}
