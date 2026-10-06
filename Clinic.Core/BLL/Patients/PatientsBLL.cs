using Clinic.Core.BLL.Patients.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    }
}
