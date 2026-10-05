using Clinic.Core.BLL.Diagnoses.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clinic.Core.BLL
{
    public class DiagnosesBLL
    {
        private readonly Entities _context;

        public DiagnosesBLL()
        {
            _context = new Entities();
        }

        public BaseResponse<List<DiagnosisDto>> GetDiagnoses()
        {
            try
            {
                var diagnoses = _context.Diagnosis
                    .Where(s => !s.IsDeleted)
                    .Select(s => new DiagnosisDto
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        IsActive = s.IsActive
                    })
                    .OrderByDescending(s => s.Id)
                    .ToList();

                return BaseResponse<List<DiagnosisDto>>.Success(diagnoses);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<DiagnosisDto>>.Fail(ex.Message);
            }
        }

        public BaseResponse<DiagnosisDto> CreateDiagnosis(DiagnosisDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<DiagnosisDto>.Fail("Names are required.");
                }

                bool exists = _context.Diagnosis.Any(s => !s.IsDeleted && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<DiagnosisDto>.Fail("Diagnosis with same name already exists.");
                }

                var diagnosis = new Diagnosi
                {
                    NameAr = dto.NameAr.Trim(),
                    NameEn = dto.NameEn.Trim(),
                    IsActive = dto.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.Diagnosis.Add(diagnosis);
                _context.SaveChanges();

                dto.Id = diagnosis.Id;
                return BaseResponse<DiagnosisDto>.Success(dto, "Diagnosis created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<DiagnosisDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<DiagnosisDto> UpdateDiagnosis(DiagnosisDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<DiagnosisDto>.Fail("Names are required.");
                }

                var diagnosis = _context.Diagnosis.FirstOrDefault(s => s.Id == dto.Id && !s.IsDeleted);
                if (diagnosis == null)
                {
                    return BaseResponse<DiagnosisDto>.Fail("Diagnosis not found.");
                }

                bool exists = _context.Diagnosis.Any(s => !s.IsDeleted && s.Id != dto.Id && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<DiagnosisDto>.Fail("Diagnosis with same name already exists.");
                }

                diagnosis.NameAr = dto.NameAr.Trim();
                diagnosis.NameEn = dto.NameEn.Trim();
                diagnosis.IsActive = dto.IsActive;
                diagnosis.ModifiedDate = DateTime.Now;
                diagnosis.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<DiagnosisDto>.Success(dto, "Diagnosis updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<DiagnosisDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<bool> DeleteDiagnosis(int id, string currentUserId)
        {
            try
            {
                var diagnosis = _context.Diagnosis.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
                if (diagnosis == null)
                {
                    return BaseResponse<bool>.Fail("Diagnosis not found.");
                }

                bool hasVisits = _context.VisitDiagnosis.Any(v => v.DiagnosisId == id);
                if (hasVisits)
                {
                    return BaseResponse<bool>.Fail("Cannot delete diagnosis because it is linked to visits.");
                }

                diagnosis.IsDeleted = true;
                diagnosis.ModifiedDate = DateTime.Now;
                diagnosis.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<bool>.Success(true, "Diagnosis deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }
    }
}
