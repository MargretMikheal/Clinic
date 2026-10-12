using Clinic.Core.BLL.Patients.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class PatientsBLL
    {
        private readonly Entities _context;

        public PatientsBLL()
        {
            _context = new Entities();
        }

        public async Task<BaseResponse<List<PatientDto>>> GetPatientsAsync()
        {
            try
            {
                var patients = await _context.Patients
                    .Where(p => !p.IsDeleted)
                    .Select(p => new PatientDto
                    {
                        Id = p.Id,
                        PatientCode = p.PatientCode,
                        Name = p.Name,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        Phone = p.Phone,
                        Email = p.Email,
                        Address = p.Address,
                        RegistrationDate = p.RegistrationDate
                    })
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                return BaseResponse<List<PatientDto>>.Success(patients);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<PatientDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<PatientDto>> GetPatientByIdAsync(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Where(p => p.Id == id && !p.IsDeleted)
                    .Select(p => new PatientDto
                    {
                        Id = p.Id,
                        PatientCode = p.PatientCode,
                        Name = p.Name,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        Phone = p.Phone,
                        Email = p.Email,
                        Address = p.Address,
                        RegistrationDate = p.RegistrationDate
                    })
                    .FirstOrDefaultAsync();

                if (patient == null) return BaseResponse<PatientDto>.Fail("Patient not found.");

                return BaseResponse<PatientDto>.Success(patient);
            }
            catch (Exception ex)
            {
                return BaseResponse<PatientDto>.Fail(ex.Message);
            }
        }

        private async Task<string> GenerateNextPatientCodeAsync()
        {
            var lastPatientCode = await _context.Patients
                .Where(p => p.PatientCode.StartsWith("PAT-"))
                .OrderByDescending(p => p.PatientCode)
                .Select(p => p.PatientCode)
                .FirstOrDefaultAsync();

            var nextNumber = 1;

            if (!string.IsNullOrEmpty(lastPatientCode))
            {
                var suffix = lastPatientCode.Substring(4);

                if (int.TryParse(suffix, out var number))
                {
                    nextNumber = number + 1;
                }
            }

            return $"PAT-{nextNumber:D6}";
        }
        public async Task<BaseResponse<bool>> CreatePatientAsync(PatientCreateDto dto, string currentUserId)
        {
            try
            {
                var generatedCode = await GenerateNextPatientCodeAsync();

                var patient = new Patient
                {
                    PatientCode = generatedCode,
                    Name = dto.Name,
                    Gender = dto.Gender,
                    BirthDate = dto.BirthDate,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    RegistrationDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(true, "Patient created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> UpdatePatientAsync(PatientUpdateDto dto, string currentUserId)
        {
            try
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == dto.Id && !p.IsDeleted);
                if (patient == null) return BaseResponse<bool>.Fail("Patient not found.");

                patient.Name = dto.Name;
                patient.Gender = dto.Gender;
                patient.BirthDate = dto.BirthDate;
                patient.Phone = dto.Phone;
                patient.Email = dto.Email;
                patient.Address = dto.Address;
                patient.ModifiedDate = DateTime.Now;
                patient.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(true, "Patient updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> DeletePatientAsync(int id, string currentUserId)
        {
            try
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
                if (patient == null) return BaseResponse<bool>.Fail("Patient not found.");

                patient.IsDeleted = true;
                patient.ModifiedDate = DateTime.Now;
                patient.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(true, "Patient deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        #region Patient History Methods

        public async Task<BaseResponse<List<PatientHistoryListItemDto>>> GetPatientsForHistoryAsync(
            string currentUserId, bool isAdmin, string searchText = null)
        {
            try
            {
                IQueryable<Patient> query = _context.Patients
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted);

                if (!isAdmin)
                {
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        return BaseResponse<List<PatientHistoryListItemDto>>.Fail("Unauthorized.");
                    }

                    var doctor = await _context.Doctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                    if (doctor == null || !doctor.IsActive)
                    {
                        return BaseResponse<List<PatientHistoryListItemDto>>.Fail("Doctor not found or inactive.");
                    }

                    int doctorId = doctor.Id;

                    query = query.Where(p =>
                        p.Appointments.Any(a => !a.IsDeleted && a.DoctorId == doctorId) ||
                        p.Visits.Any(v => !v.IsDeleted && !v.Appointment.IsDeleted && v.DoctorId == doctorId));
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    query = query.Where(p => p.Name.Contains(searchText) ||
                                             p.PatientCode.Contains(searchText) ||
                                             p.Phone.Contains(searchText));
                }

                var patients = await query
                    .OrderByDescending(p => p.Id)
                    .Select(p => new PatientHistoryListItemDto
                    {
                        Id = p.Id,
                        PatientCode = p.PatientCode,
                        Name = p.Name,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        Phone = p.Phone,
                        Email = p.Email,
                        RegistrationDate = p.RegistrationDate
                    })
                    .ToListAsync();

                return BaseResponse<List<PatientHistoryListItemDto>>.Success(patients);
            }
            catch (Exception ex)
            {
                string errorId = Guid.NewGuid().ToString();
                Trace.TraceError($"[ErrorId: {errorId}] Error in PatientsBLL.GetPatientsForHistoryAsync: {ex}");
                return new BaseResponse<List<PatientHistoryListItemDto>>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred while retrieving patients.",
                    ErrorId = errorId
                };
            }
        }

        public async Task<BaseResponse<PatientHistoryDetailsDto>> GetPatientHistoryDetailsAsync(int patientId, string currentUserId, bool isAdmin)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BaseResponse<PatientHistoryDetailsDto>.Fail("Invalid patient ID.");
                }

                int doctorId = 0;
                if (!isAdmin)
                {
                    if (string.IsNullOrWhiteSpace(currentUserId))
                    {
                        return BaseResponse<PatientHistoryDetailsDto>.Fail("Unauthorized.");
                    }

                    var doctor = await _context.Doctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                    if (doctor == null || !doctor.IsActive)
                    {
                        return BaseResponse<PatientHistoryDetailsDto>.Fail("Doctor not found or inactive.");
                    }

                    doctorId = doctor.Id;
                }

                IQueryable<Patient> patientQuery = _context.Patients
                    .AsNoTracking()
                    .Where(p => p.Id == patientId && !p.IsDeleted);

                if (!isAdmin)
                {
                    patientQuery = patientQuery.Where(p =>
                        p.Appointments.Any(a => !a.IsDeleted && a.DoctorId == doctorId) ||
                        p.Visits.Any(v => !v.IsDeleted && !v.Appointment.IsDeleted && v.DoctorId == doctorId));
                }

                var patient = await patientQuery
                    .Select(p => new PatientDto
                    {
                        Id = p.Id,
                        PatientCode = p.PatientCode,
                        Name = p.Name,
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        Phone = p.Phone,
                        Email = p.Email,
                        Address = p.Address,
                        RegistrationDate = p.RegistrationDate
                    })
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    return BaseResponse<PatientHistoryDetailsDto>.Fail("Patient not found.");
                }

                var visits = await _context.Visits
                    .AsNoTracking()
                    .Where(v => v.PatientId == patientId &&
                                !v.IsDeleted &&
                                !v.Appointment.IsDeleted)
                    .OrderByDescending(v => v.VisitDate)
                    .ThenByDescending(v => v.Appointment.AppointmentTime)
                    .ThenByDescending(v => v.Id)
                    .Select(v => new VisitHistoryItemDto
                    {
                        Id = v.Id,
                        AppointmentId = v.AppointmentId,
                        VisitDate = v.VisitDate,
                        AppointmentDate = v.Appointment.AppointmentDate,
                        AppointmentTime = v.Appointment.AppointmentTime,
                        AppointmentStatus = v.Appointment.Status,
                        DoctorId = v.DoctorId,
                        DoctorName = v.Doctor.Name,
                        DoctorCode = v.Doctor.DoctorCode,
                        SpecialityNameAr = v.Doctor.Speciality != null ? v.Doctor.Speciality.NameAr : null,
                        SpecialityNameEn = v.Doctor.Speciality != null ? v.Doctor.Speciality.NameEn : null,
                        Notes = v.Notes,
                        Symptoms = v.VisitSymptoms
                            .Where(vs => !vs.Symptom.IsDeleted)
                            .Select(vs => new SymptomHistoryDto
                            {
                                Id = vs.Symptom.Id,
                                NameAr = vs.Symptom.NameAr,
                                NameEn = vs.Symptom.NameEn
                            })
                            .ToList(),
                        Diagnoses = v.VisitDiagnosis
                            .Where(vd => !vd.Diagnosi.IsDeleted)
                            .Select(vd => new DiagnosisHistoryDto
                            {
                                Id = vd.Diagnosi.Id,
                                NameAr = vd.Diagnosi.NameAr,
                                NameEn = vd.Diagnosi.NameEn
                            })
                            .ToList(),
                        Prescriptions = v.Prescriptions
                            .Where(p => !p.IsDeleted)
                            .Select(p => new PrescriptionHistoryDto
                            {
                                Id = p.Id,
                                Notes = p.Notes,
                                CreatedDate = p.CreatedDate,
                                Items = p.PrescriptionItems
                                    .Where(pi => !pi.IsDeleted)
                                    .Select(pi => new PrescriptionItemHistoryDto
                                    {
                                        Id = pi.Id,
                                        MedicineName = pi.MedicineName,
                                        Dosage = pi.Dosage,
                                        Frequency = pi.Frequency,
                                        Duration = pi.Duration,
                                        Notes = pi.Notes
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToListAsync();

                var responseData = new PatientHistoryDetailsDto
                {
                    Patient = patient,
                    Visits = visits ?? new List<VisitHistoryItemDto>()
                };

                return BaseResponse<PatientHistoryDetailsDto>.Success(responseData);
            }
            catch (Exception ex)
            {
                string errorId = Guid.NewGuid().ToString();
                Trace.TraceError($"[ErrorId: {errorId}] Error in PatientsBLL.GetPatientHistoryDetailsAsync for PatientId {patientId}: {ex}");
                return new BaseResponse<PatientHistoryDetailsDto>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred while retrieving patient history details.",
                    ErrorId = errorId
                };
            }
        }

        #endregion
    }
}
