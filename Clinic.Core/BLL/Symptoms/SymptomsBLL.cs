using Clinic.Core.BLL.Symptoms.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class SymptomsBLL
    {
        private readonly Entities _context;

        public SymptomsBLL()
        {
            _context = new Entities();
        }

        public BaseResponse<List<SymptomDto>> GetSymptoms()
        {
            try
            {
                var symptoms = _context.Symptoms
                    .Where(s => !s.IsDeleted)
                    .Select(s => new SymptomDto
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        IsActive = s.IsActive
                    })
                    .OrderByDescending(s => s.Id)
                    .ToList();

                return BaseResponse<List<SymptomDto>>.Success(symptoms);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<SymptomDto>>.Fail(ex.Message);
            }
        }

        public BaseResponse<SymptomDto> CreateSymptom(SymptomDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<SymptomDto>.Fail("Names are required.");
                }

                bool exists = _context.Symptoms.Any(s => !s.IsDeleted && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<SymptomDto>.Fail("Symptom with same name already exists.");
                }

                var symptom = new Symptom
                {
                    NameAr = dto.NameAr.Trim(),
                    NameEn = dto.NameEn.Trim(),
                    IsActive = dto.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.Symptoms.Add(symptom);
                _context.SaveChanges();

                dto.Id = symptom.Id;
                return BaseResponse<SymptomDto>.Success(dto, "Symptom created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<SymptomDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<SymptomDto> UpdateSymptom(SymptomDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<SymptomDto>.Fail("Names are required.");
                }

                var symptom = _context.Symptoms.FirstOrDefault(s => s.Id == dto.Id && !s.IsDeleted);
                if (symptom == null)
                {
                    return BaseResponse<SymptomDto>.Fail("Symptom not found.");
                }

                bool exists = _context.Symptoms.Any(s => !s.IsDeleted && s.Id != dto.Id && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<SymptomDto>.Fail("Symptom with same name already exists.");
                }

                symptom.NameAr = dto.NameAr.Trim();
                symptom.NameEn = dto.NameEn.Trim();
                symptom.IsActive = dto.IsActive;
                symptom.ModifiedDate = DateTime.Now;
                symptom.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<SymptomDto>.Success(dto, "Symptom updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<SymptomDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<bool> DeleteSymptom(int id, string currentUserId)
        {
            try
            {
                var symptom = _context.Symptoms.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
                if (symptom == null)
                {
                    return BaseResponse<bool>.Fail("Symptom not found.");
                }

                // Check dependencies
                bool hasVisits = _context.VisitSymptoms.Any(v => v.SymptomId == id);
                if (hasVisits)
                {
                    return BaseResponse<bool>.Fail("Cannot delete symptom because it is linked to visits.");
                }

                symptom.IsDeleted = true;
                symptom.ModifiedDate = DateTime.Now;
                symptom.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<bool>.Success(true, "Symptom deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }
    }
}
