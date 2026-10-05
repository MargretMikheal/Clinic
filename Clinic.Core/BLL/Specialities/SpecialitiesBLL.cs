using Clinic.Core.BLL.Specialities.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class SpecialitiesBLL
    {
        private readonly Entities _context;

        public SpecialitiesBLL()
        {
            _context = new Entities();
        }

        public BaseResponse<List<SpecialityDto>> GetSpecialities()
        {
            try
            {
                var specialities = _context.Specialities
                    .Where(s => !s.IsDeleted)
                    .Select(s => new SpecialityDto
                    {
                        Id = s.Id,
                        NameAr = s.NameAr,
                        NameEn = s.NameEn,
                        IsActive = s.IsActive
                    })
                    .OrderByDescending(s => s.Id)
                    .ToList();

                return BaseResponse<List<SpecialityDto>>.Success(specialities);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<SpecialityDto>>.Fail(ex.Message);
            }
        }

        public BaseResponse<SpecialityDto> CreateSpeciality(SpecialityDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<SpecialityDto>.Fail("Names are required.");
                }

                bool exists = _context.Specialities.Any(s => !s.IsDeleted && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<SpecialityDto>.Fail("Speciality with same name already exists.");
                }

                var speciality = new Speciality
                {
                    NameAr = dto.NameAr.Trim(),
                    NameEn = dto.NameEn.Trim(),
                    IsActive = dto.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.Specialities.Add(speciality);
                _context.SaveChanges();

                dto.Id = speciality.Id;
                return BaseResponse<SpecialityDto>.Success(dto, "Speciality created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<SpecialityDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<SpecialityDto> UpdateSpeciality(SpecialityDto dto, string currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
                {
                    return BaseResponse<SpecialityDto>.Fail("Names are required.");
                }

                var speciality = _context.Specialities.FirstOrDefault(s => s.Id == dto.Id && !s.IsDeleted);
                if (speciality == null)
                {
                    return BaseResponse<SpecialityDto>.Fail("Speciality not found.");
                }

                bool exists = _context.Specialities.Any(s => !s.IsDeleted && s.Id != dto.Id && (s.NameAr == dto.NameAr || s.NameEn == dto.NameEn));
                if (exists)
                {
                    return BaseResponse<SpecialityDto>.Fail("Speciality with same name already exists.");
                }

                speciality.NameAr = dto.NameAr.Trim();
                speciality.NameEn = dto.NameEn.Trim();
                speciality.IsActive = dto.IsActive;
                speciality.ModifiedDate = DateTime.Now;
                speciality.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<SpecialityDto>.Success(dto, "Speciality updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<SpecialityDto>.Fail(ex.Message);
            }
        }

        public BaseResponse<bool> DeleteSpeciality(int id, string currentUserId)
        {
            try
            {
                var speciality = _context.Specialities.FirstOrDefault(s => s.Id == id && !s.IsDeleted);
                if (speciality == null)
                {
                    return BaseResponse<bool>.Fail("Speciality not found.");
                }

                // Check dependencies (e.g. Doctors)
                bool hasDoctors = _context.Doctors.Any(d => d.SpecialityId == id && !d.IsDeleted);
                if (hasDoctors)
                {
                    return BaseResponse<bool>.Fail("Cannot delete speciality because it is assigned to doctors.");
                }

                speciality.IsDeleted = true;
                speciality.ModifiedDate = DateTime.Now;
                speciality.ModifiedBy = currentUserId;

                _context.SaveChanges();

                return BaseResponse<bool>.Success(true, "Speciality deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }
    }
}
