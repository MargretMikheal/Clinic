using Clinic.Core.BLL.Doctors.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Identity;
using Clinic.Core.Identity.Stores;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL
{
    public class DoctorsBLL
    {
        private readonly Entities _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorsBLL()
        {
            _context = new Entities();
            var store = new ApplicationUserStore(_context);
            _userManager = new UserManager<ApplicationUser>(store);
        }

        public async Task<BaseResponse<List<DoctorListDto>>> GetDoctorsAsync()
        {
            try
            {
                var doctors = _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .ToList()
                    .Select(d => new DoctorListDto
                    {
                        Id = d.Id,
                        DoctorCode = d.DoctorCode,
                        Name = d.Name,
                        SpecialityName = d.Speciality != null ? d.Speciality.NameEn : "",
                        Phone = d.Phone,
                        Email = d.Email,
                        ExaminationPrice = d.ExaminationPrice,
                        SlotDuration = d.SlotDuration,
                        IsActive = d.IsActive,
                        ReceptionistName = d.ReceptionistUserId != null ? _context.AspNetUsers.FirstOrDefault(u => u.Id == d.ReceptionistUserId.ToString())?.UserName : ""
                    }).ToList();

                return BaseResponse<List<DoctorListDto>>.Success(doctors);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<DoctorListDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<DoctorUpdateDto>> GetDoctorByIdAsync(int id)
        {
            try
            {
                var doctor = _context.Doctors
                    .Include(d => d.DoctorSchedules)
                    .FirstOrDefault(d => d.Id == id && !d.IsDeleted);

                if (doctor == null) return BaseResponse<DoctorUpdateDto>.Fail("Doctor not found.");

                var dto = new DoctorUpdateDto
                {
                    Id = doctor.Id,
                    UserId = doctor.UserId,
                    UserName = _context.AspNetUsers.FirstOrDefault(u => u.Id == doctor.UserId)?.UserName,
                    Email = doctor.Email,
                    DoctorCode = doctor.DoctorCode,
                    Name = doctor.Name,
                    SpecialityId = doctor.SpecialityId,
                    Phone = doctor.Phone,
                    ExaminationPrice = doctor.ExaminationPrice,
                    SlotDuration = doctor.SlotDuration,
                    IsActive = doctor.IsActive,
                    ReceptionistUserId = doctor.ReceptionistUserId?.ToString(),
                    Schedules = doctor.DoctorSchedules.Where(s => !s.IsDeleted).Select(s => new DoctorScheduleDto
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime.ToString(@"hh\:mm\:ss"),
                        EndTime = s.EndTime.ToString(@"hh\:mm\:ss")
                    }).ToList()
                };

                return BaseResponse<DoctorUpdateDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return BaseResponse<DoctorUpdateDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<List<DoctorSpecialityOptionDto>>> GetDoctorSpecialitiesAsync()
        {
            try
            {
                var specialities = _context.Specialities
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .Select(s => new DoctorSpecialityOptionDto
                    {
                        Id = s.Id,
                        Name = s.NameEn
                    }).ToList();
                return BaseResponse<List<DoctorSpecialityOptionDto>>.Success(specialities);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<DoctorSpecialityOptionDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<string>> GetNextDoctorCodeAsync()
        {
            try
            {
                var code = await GenerateNextDoctorCodeAsync();
                return BaseResponse<string>.Success(code);
            }
            catch (Exception ex)
            {
                return BaseResponse<string>.Fail(ex.Message);
            }
        }

        private async Task<string> GenerateNextDoctorCodeAsync()
        {
            var doctorCodes = await _context.Doctors
                .Select(d => d.DoctorCode)
                .ToListAsync();

            int maxSuffix = 0;
            foreach (var code in doctorCodes)
            {
                if (!string.IsNullOrEmpty(code) && code.StartsWith("DOC-"))
                {
                    var suffixString = code.Substring(4);
                    if (int.TryParse(suffixString, out int suffix))
                    {
                        if (suffix > maxSuffix)
                        {
                            maxSuffix = suffix;
                        }
                    }
                }
            }

            return $"DOC-{(maxSuffix + 1).ToString("D3")}";
        }

        public async Task<BaseResponse<List<ReceptionistAssignmentOptionDto>>> GetDoctorReceptionistsAsync()
        {
            try
            {
                var roleName = nameof(Clinic.Core.Enums.UserRole.Receptionist);
                var role = _context.AspNetRoles.FirstOrDefault(r => r.Name == roleName);
                if (role == null) return BaseResponse<List<ReceptionistAssignmentOptionDto>>.Success(new List<ReceptionistAssignmentOptionDto>());

                var users = _context.AspNetUsers
                    .Where(u => u.AspNetRoles.Any(r => r.Id == role.Id))
                    .Select(u => new ReceptionistAssignmentOptionDto
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        Email = u.Email
                    }).ToList();
                return BaseResponse<List<ReceptionistAssignmentOptionDto>>.Success(users);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<ReceptionistAssignmentOptionDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> CreateDoctorAsync(DoctorCreateDto dto, string currentUserId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (dto.Schedules != null && dto.Schedules.Any())
                    {
                        foreach (var schedule in dto.Schedules)
                        {
                            if (TimeSpan.Parse(schedule.StartTime) >= TimeSpan.Parse(schedule.EndTime))
                                return BaseResponse<bool>.Fail("Start time must be before end time.");
                        }
                        var overlaps = dto.Schedules.GroupBy(s => s.DayOfWeek).Any(g => 
                            g.Any(s1 => g.Any(s2 => s1 != s2 && TimeSpan.Parse(s1.StartTime) < TimeSpan.Parse(s2.EndTime) && TimeSpan.Parse(s2.StartTime) < TimeSpan.Parse(s1.EndTime))));
                        if (overlaps) return BaseResponse<bool>.Fail("Overlapping schedules are not allowed.");
                    }

                    Guid? receptionistGuid = null;
                    if (!string.IsNullOrEmpty(dto.ReceptionistUserId))
                    {
                        if (Guid.TryParse(dto.ReceptionistUserId, out var parsed))
                        {
                            var existingAssigned = _context.Doctors.Any(d => d.ReceptionistUserId == parsed && !d.IsDeleted);
                            if (existingAssigned) return BaseResponse<bool>.Fail("Receptionist is already assigned to another doctor.");
                            receptionistGuid = parsed;
                        }
                        else return BaseResponse<bool>.Fail("Invalid receptionist ID format.");
                    }

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = dto.UserName,
                        Email = dto.Email
                    };
                    var result = await _userManager.CreateAsync(user, dto.Password);
                    if (!result.Succeeded) return BaseResponse<bool>.Fail(string.Join(", ", result.Errors));

                    var roleResult = await _userManager.AddToRoleAsync(user.Id, nameof(Clinic.Core.Enums.UserRole.Doctor));
                    if (!roleResult.Succeeded) return BaseResponse<bool>.Fail("Failed to assign Doctor role.");

                    var generatedCode = await GenerateNextDoctorCodeAsync();

                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        DoctorCode = generatedCode,
                        Name = dto.Name,
                        SpecialityId = dto.SpecialityId,
                        Phone = dto.Phone,
                        Email = dto.Email,
                        ExaminationPrice = dto.ExaminationPrice,
                        SlotDuration = dto.SlotDuration,
                        IsActive = dto.IsActive,
                        ReceptionistUserId = receptionistGuid,
                        CreatedDate = DateTime.Now,
                        CreatedBy = currentUserId,
                        IsDeleted = false
                    };

                    _context.Doctors.Add(doctor);
                    _context.SaveChanges();

                    if (dto.Schedules != null)
                    {
                        foreach (var s in dto.Schedules)
                        {
                            _context.DoctorSchedules.Add(new DoctorSchedule
                            {
                                DoctorId = doctor.Id,
                                DayOfWeek = s.DayOfWeek,
                                StartTime = TimeSpan.Parse(s.StartTime),
                                EndTime = TimeSpan.Parse(s.EndTime),
                                CreatedDate = DateTime.Now,
                                CreatedBy = currentUserId,
                                IsDeleted = false
                            });
                        }
                        _context.SaveChanges();
                    }

                    transaction.Commit();
                    return BaseResponse<bool>.Success(true, "Doctor created successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BaseResponse<bool>.Fail(ex.Message);
                }
            }
        }

        public async Task<BaseResponse<bool>> UpdateDoctorAsync(DoctorUpdateDto dto, string currentUserId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var doctor = _context.Doctors.Include(d => d.DoctorSchedules).FirstOrDefault(d => d.Id == dto.Id && !d.IsDeleted);
                    if (doctor == null) return BaseResponse<bool>.Fail("Doctor not found.");

                    if (dto.Schedules != null && dto.Schedules.Any())
                    {
                        foreach (var schedule in dto.Schedules)
                        {
                            if (TimeSpan.Parse(schedule.StartTime) >= TimeSpan.Parse(schedule.EndTime))
                                return BaseResponse<bool>.Fail("Start time must be before end time.");
                        }
                        var overlaps = dto.Schedules.GroupBy(s => s.DayOfWeek).Any(g => 
                            g.Any(s1 => g.Any(s2 => s1 != s2 && TimeSpan.Parse(s1.StartTime) < TimeSpan.Parse(s2.EndTime) && TimeSpan.Parse(s2.StartTime) < TimeSpan.Parse(s1.EndTime))));
                        if (overlaps) return BaseResponse<bool>.Fail("Overlapping schedules are not allowed.");
                    }

                    Guid? receptionistGuid = null;
                    if (!string.IsNullOrEmpty(dto.ReceptionistUserId))
                    {
                        if (Guid.TryParse(dto.ReceptionistUserId, out var parsed))
                        {
                            var existingAssigned = _context.Doctors.Any(d => d.Id != doctor.Id && d.ReceptionistUserId == parsed && !d.IsDeleted);
                            if (existingAssigned) return BaseResponse<bool>.Fail("Receptionist is already assigned to another doctor.");
                            receptionistGuid = parsed;
                        }
                        else return BaseResponse<bool>.Fail("Invalid receptionist ID format.");
                    }

                    doctor.Name = dto.Name;
                    doctor.SpecialityId = dto.SpecialityId;
                    doctor.Phone = dto.Phone;
                    doctor.Email = dto.Email;
                    doctor.ExaminationPrice = dto.ExaminationPrice;
                    doctor.SlotDuration = dto.SlotDuration;
                    doctor.IsActive = dto.IsActive;
                    doctor.ReceptionistUserId = receptionistGuid;
                    doctor.ModifiedDate = DateTime.Now;
                    doctor.ModifiedBy = currentUserId;

                    var existingSchedules = doctor.DoctorSchedules.Where(s => !s.IsDeleted).ToList();
                    var newSchedules = dto.Schedules ?? new List<DoctorScheduleDto>();

                    foreach (var es in existingSchedules)
                    {
                        if (!newSchedules.Any(ns => ns.Id == es.Id))
                        {
                            es.IsDeleted = true;
                            es.ModifiedDate = DateTime.Now;
                            es.ModifiedBy = currentUserId;
                        }
                    }

                    foreach (var ns in newSchedules)
                    {
                        if (ns.Id == 0)
                        {
                            _context.DoctorSchedules.Add(new DoctorSchedule
                            {
                                DoctorId = doctor.Id,
                                DayOfWeek = ns.DayOfWeek,
                                StartTime = TimeSpan.Parse(ns.StartTime),
                                EndTime = TimeSpan.Parse(ns.EndTime),
                                CreatedDate = DateTime.Now,
                                CreatedBy = currentUserId,
                                IsDeleted = false
                            });
                        }
                        else
                        {
                            var es = existingSchedules.FirstOrDefault(s => s.Id == ns.Id);
                            if (es != null)
                            {
                                es.DayOfWeek = ns.DayOfWeek;
                                es.StartTime = TimeSpan.Parse(ns.StartTime);
                                es.EndTime = TimeSpan.Parse(ns.EndTime);
                                es.ModifiedDate = DateTime.Now;
                                es.ModifiedBy = currentUserId;
                            }
                        }
                    }

                    var user = await _userManager.FindByIdAsync(doctor.UserId);
                    if (user != null && user.Email != dto.Email)
                    {
                        user.Email = dto.Email;
                        await _userManager.UpdateAsync(user);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return BaseResponse<bool>.Success(true, "Doctor updated successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BaseResponse<bool>.Fail(ex.Message);
                }
            }
        }
    }
}
