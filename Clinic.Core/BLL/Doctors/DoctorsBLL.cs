using Clinic.Core.BLL.Doctors.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Enums;
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
                var doctors = await _context.Doctors
                    .AsNoTracking()
                    .Where(d => !d.IsDeleted)
                    .Select(d => new
                    {
                        d.Id,
                        d.DoctorCode,
                        d.Name,
                        SpecialityName = d.Speciality != null ? d.Speciality.NameEn : "",
                        d.Phone,
                        d.Email,
                        d.ExaminationPrice,
                        d.SlotDuration,
                        d.IsActive,
                        d.ReceptionistUserId
                    })
                    .ToListAsync();

                var receptionistIds = doctors
                    .Where(d => d.ReceptionistUserId.HasValue)
                    .Select(d => d.ReceptionistUserId.Value.ToString())
                    .Distinct()
                    .ToList();

                var receptionistMap = receptionistIds.Any()
                    ? await _context.AspNetUsers
                        .AsNoTracking()
                        .Where(u => receptionistIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.UserName })
                        .ToDictionaryAsync(u => u.Id, u => u.UserName)
                    : new Dictionary<string, string>();

                var result = doctors.Select(d => new DoctorListDto
                {
                    Id = d.Id,
                    DoctorCode = d.DoctorCode,
                    Name = d.Name,
                    SpecialityName = d.SpecialityName,
                    Phone = d.Phone,
                    Email = d.Email,
                    ExaminationPrice = d.ExaminationPrice,
                    SlotDuration = d.SlotDuration,
                    IsActive = d.IsActive,
                    ReceptionistName = (d.ReceptionistUserId.HasValue && receptionistMap.TryGetValue(d.ReceptionistUserId.Value.ToString(), out var rName))
                        ? rName
                        : ""
                }).ToList();

                return BaseResponse<List<DoctorListDto>>.Success(result);
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
                var roleName = "Receptionist";
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

                    var roleResult = await _userManager.AddToRoleAsync(user.Id, "Doctor");
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

                    if (dto.Schedules != null)
                    {
                        var existingSchedules = doctor.DoctorSchedules.Where(s => !s.IsDeleted).ToList();

                        foreach (var es in existingSchedules)
                        {
                            if (!dto.Schedules.Any(ns => ns.Id == es.Id))
                            {
                                es.IsDeleted = true;
                                es.ModifiedDate = DateTime.Now;
                                es.ModifiedBy = currentUserId;
                            }
                        }

                        foreach (var ns in dto.Schedules)
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

        #region Doctor My Schedule Methods

        public async Task<BaseResponse<DoctorWeeklyScheduleDto>> GetWeeklyScheduleAsync(string currentUserId, bool isAdmin, int? targetDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return BaseResponse<DoctorWeeklyScheduleDto>.Fail("schedule.Unauthorized");

                Doctor doctor = null;

                if (isAdmin)
                {
                    if (!targetDoctorId.HasValue || targetDoctorId.Value <= 0)
                    {
                        // Admin has not selected a doctor yet - return empty success
                        return BaseResponse<DoctorWeeklyScheduleDto>.Success(null);
                    }

                    doctor = await _context.Doctors
                        .Include(d => d.DoctorSchedules)
                        .FirstOrDefaultAsync(d => d.Id == targetDoctorId.Value && !d.IsDeleted);

                    if (doctor == null)
                        return BaseResponse<DoctorWeeklyScheduleDto>.Fail("schedule.DoctorNotFound");

                    if (!doctor.IsActive)
                        return BaseResponse<DoctorWeeklyScheduleDto>.Fail("schedule.DoctorInactive");
                }
                else
                {
                    // Doctor mode: resolve strictly by CurrentUserId
                    doctor = await _context.Doctors
                        .Include(d => d.DoctorSchedules)
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);

                    if (doctor == null)
                        return BaseResponse<DoctorWeeklyScheduleDto>.Fail("schedule.DoctorNotFound");

                    if (!doctor.IsActive)
                        return BaseResponse<DoctorWeeklyScheduleDto>.Fail("schedule.DoctorInactive");
                }

                var schedules = doctor.DoctorSchedules
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Select(s => new DoctorScheduleDto
                    {
                        Id = s.Id,
                        DoctorId = s.DoctorId,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime.ToString(@"hh\:mm"),
                        EndTime = s.EndTime.ToString(@"hh\:mm")
                    }).ToList();

                var result = new DoctorWeeklyScheduleDto
                {
                    DoctorId = doctor.Id,
                    DoctorCode = doctor.DoctorCode,
                    DoctorName = doctor.Name,
                    SlotDuration = doctor.SlotDuration,
                    Schedules = schedules
                };

                return BaseResponse<DoctorWeeklyScheduleDto>.Success(result);
            }
            catch (Exception ex)
            {
                return BaseResponse<DoctorWeeklyScheduleDto>.Fail(ex.Message);
            }
        }

        public Task<BaseResponse<DoctorWeeklyScheduleDto>> GetMyWeeklyScheduleAsync(string currentUserId)
        {
            return GetWeeklyScheduleAsync(currentUserId, false, null);
        }

        public async Task<BaseResponse<List<DoctorListDto>>> GetActiveDoctorsForScheduleAsync()
        {
            try
            {
                var doctors = await _context.Doctors
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new DoctorListDto
                    {
                        Id = d.Id,
                        DoctorCode = d.DoctorCode,
                        Name = d.Name,
                        SlotDuration = d.SlotDuration,
                        IsActive = d.IsActive
                    })
                    .ToListAsync();

                return BaseResponse<List<DoctorListDto>>.Success(doctors);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<DoctorListDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<DoctorScheduleDto>> AddSchedulePeriodAsync(DoctorScheduleDto dto, string currentUserId, bool isAdmin = false, int? targetDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.Unauthorized");

                Doctor doctor = null;
                if (isAdmin)
                {
                    int docId = targetDoctorId ?? (dto != null ? dto.DoctorId : 0);
                    if (docId <= 0)
                        return BaseResponse<DoctorScheduleDto>.Fail("schedule.PleaseSelectDoctor");

                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.Id == docId && !d.IsDeleted);
                }
                else
                {
                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);
                }

                if (doctor == null)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.DoctorNotFound");

                if (!doctor.IsActive)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.DoctorInactive");

                if (dto == null)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.InvalidData");

                if (dto.DayOfWeek > 6)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.InvalidDayOfWeek");

                if (!TimeSpan.TryParse(dto.StartTime, out var startTime) || !TimeSpan.TryParse(dto.EndTime, out var endTime))
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.InvalidTimeFormat");

                if (startTime >= endTime)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.InvalidTimeRange");

                if (doctor.SlotDuration > 0 && (endTime - startTime).TotalMinutes < doctor.SlotDuration)
                    return BaseResponse<DoctorScheduleDto>.Fail($"schedule.PeriodShorterThanSlot::{doctor.SlotDuration}");

                var existingSchedules = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == dto.DayOfWeek && !s.IsDeleted)
                    .ToListAsync();

                var hasOverlap = existingSchedules.Any(s => startTime < s.EndTime && s.StartTime < endTime);
                if (hasOverlap)
                    return BaseResponse<DoctorScheduleDto>.Fail("schedule.OverlappingPeriod");

                var newSchedule = new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.DoctorSchedules.Add(newSchedule);
                await _context.SaveChangesAsync();

                var createdDto = new DoctorScheduleDto
                {
                    Id = newSchedule.Id,
                    DoctorId = newSchedule.DoctorId,
                    DayOfWeek = newSchedule.DayOfWeek,
                    StartTime = newSchedule.StartTime.ToString(@"hh\:mm"),
                    EndTime = newSchedule.EndTime.ToString(@"hh\:mm")
                };

                return BaseResponse<DoctorScheduleDto>.Success(createdDto, "schedule.PeriodSavedSuccess");
            }
            catch (Exception ex)
            {
                return BaseResponse<DoctorScheduleDto>.Fail(ex.Message);
            }
        }

        public Task<BaseResponse<DoctorScheduleDto>> AddSchedulePeriodAsync(DoctorScheduleDto dto, string currentUserId)
        {
            return AddSchedulePeriodAsync(dto, currentUserId, false, null);
        }

        public async Task<BaseResponse<bool>> UpdateSchedulePeriodAsync(DoctorScheduleDto dto, string currentUserId, bool isAdmin = false, int? targetDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return BaseResponse<bool>.Fail("schedule.Unauthorized");

                if (dto == null || dto.Id <= 0)
                    return BaseResponse<bool>.Fail("schedule.InvalidData");

                Doctor doctor = null;
                if (isAdmin)
                {
                    int docId = targetDoctorId ?? dto.DoctorId;
                    if (docId <= 0)
                        return BaseResponse<bool>.Fail("schedule.PleaseSelectDoctor");

                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.Id == docId && !d.IsDeleted);
                }
                else
                {
                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);
                }

                if (doctor == null)
                    return BaseResponse<bool>.Fail("schedule.DoctorNotFound");

                if (!doctor.IsActive)
                    return BaseResponse<bool>.Fail("schedule.DoctorInactive");

                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted);

                if (schedule == null)
                    return BaseResponse<bool>.Fail("schedule.PeriodNotFound");

                if (schedule.DoctorId != doctor.Id)
                    return BaseResponse<bool>.Fail("schedule.Unauthorized");

                if (dto.DayOfWeek > 6)
                    return BaseResponse<bool>.Fail("schedule.InvalidDayOfWeek");

                if (!TimeSpan.TryParse(dto.StartTime, out var startTime) || !TimeSpan.TryParse(dto.EndTime, out var endTime))
                    return BaseResponse<bool>.Fail("schedule.InvalidTimeFormat");

                if (startTime >= endTime)
                    return BaseResponse<bool>.Fail("schedule.InvalidTimeRange");

                if (doctor.SlotDuration > 0 && (endTime - startTime).TotalMinutes < doctor.SlotDuration)
                    return BaseResponse<bool>.Fail($"schedule.PeriodShorterThanSlot::{doctor.SlotDuration}");

                var otherSchedulesOnNewDay = await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == dto.DayOfWeek && s.Id != dto.Id && !s.IsDeleted)
                    .ToListAsync();

                var hasOverlap = otherSchedulesOnNewDay.Any(s => startTime < s.EndTime && s.StartTime < endTime);
                if (hasOverlap)
                    return BaseResponse<bool>.Fail("schedule.OverlappingPeriod");

                // Protect existing future appointments
                var resultingNewDaySchedules = otherSchedulesOnNewDay
                    .Select(s => new SchedulePeriodRange(s.StartTime, s.EndTime))
                    .ToList();
                resultingNewDaySchedules.Add(new SchedulePeriodRange(startTime, endTime));

                var conflictMessage = await ValidateAppointmentsFitScheduleAsync(doctor.Id, doctor.SlotDuration, dto.DayOfWeek, resultingNewDaySchedules);
                if (conflictMessage != null)
                    return BaseResponse<bool>.Fail(conflictMessage);

                // If day of week changed, check that previous day's appointments are still valid with this period removed
                if (schedule.DayOfWeek != dto.DayOfWeek)
                {
                    var remainingOldDaySchedules = (await _context.DoctorSchedules
                        .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == schedule.DayOfWeek && s.Id != dto.Id && !s.IsDeleted)
                        .ToListAsync())
                        .Select(s => new SchedulePeriodRange(s.StartTime, s.EndTime))
                        .ToList();

                    var oldDayConflict = await ValidateAppointmentsFitScheduleAsync(doctor.Id, doctor.SlotDuration, schedule.DayOfWeek, remainingOldDaySchedules);
                    if (oldDayConflict != null)
                        return BaseResponse<bool>.Fail(oldDayConflict);
                }

                schedule.DayOfWeek = dto.DayOfWeek;
                schedule.StartTime = startTime;
                schedule.EndTime = endTime;
                schedule.ModifiedDate = DateTime.Now;
                schedule.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();
                return BaseResponse<bool>.Success(true, "schedule.PeriodSavedSuccess");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public Task<BaseResponse<bool>> UpdateSchedulePeriodAsync(DoctorScheduleDto dto, string currentUserId)
        {
            return UpdateSchedulePeriodAsync(dto, currentUserId, false, null);
        }

        public async Task<BaseResponse<bool>> DeleteSchedulePeriodAsync(int periodId, string currentUserId, bool isAdmin = false, int? targetDoctorId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return BaseResponse<bool>.Fail("schedule.Unauthorized");

                if (periodId <= 0)
                    return BaseResponse<bool>.Fail("schedule.PeriodNotFound");

                Doctor doctor = null;
                if (isAdmin)
                {
                    int docId = targetDoctorId.GetValueOrDefault();
                    if (docId <= 0)
                        return BaseResponse<bool>.Fail("schedule.PleaseSelectDoctor");

                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.Id == docId && !d.IsDeleted);
                }
                else
                {
                    doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId && !d.IsDeleted);
                }

                if (doctor == null)
                    return BaseResponse<bool>.Fail("schedule.DoctorNotFound");

                if (!doctor.IsActive)
                    return BaseResponse<bool>.Fail("schedule.DoctorInactive");

                var schedule = await _context.DoctorSchedules
                    .FirstOrDefaultAsync(s => s.Id == periodId && !s.IsDeleted);

                if (schedule == null)
                    return BaseResponse<bool>.Fail("schedule.PeriodNotFound");

                if (schedule.DoctorId != doctor.Id)
                    return BaseResponse<bool>.Fail("schedule.Unauthorized");

                // Check appointment protection: would remaining periods on this day fit all future booked/confirmed appointments?
                var remainingSchedules = (await _context.DoctorSchedules
                    .Where(s => s.DoctorId == doctor.Id && s.DayOfWeek == schedule.DayOfWeek && s.Id != periodId && !s.IsDeleted)
                    .ToListAsync())
                    .Select(s => new SchedulePeriodRange(s.StartTime, s.EndTime))
                    .ToList();

                var conflictMessage = await ValidateAppointmentsFitScheduleAsync(doctor.Id, doctor.SlotDuration, schedule.DayOfWeek, remainingSchedules);
                if (conflictMessage != null)
                    return BaseResponse<bool>.Fail(conflictMessage);

                schedule.IsDeleted = true;
                schedule.ModifiedDate = DateTime.Now;
                schedule.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();
                return BaseResponse<bool>.Success(true, "schedule.PeriodDeletedSuccess");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public Task<BaseResponse<bool>> DeleteSchedulePeriodAsync(int periodId, string currentUserId)
        {
            return DeleteSchedulePeriodAsync(periodId, currentUserId, false, null);
        }

        private async Task<string> ValidateAppointmentsFitScheduleAsync(int doctorId, int slotDuration, byte dayOfWeek, List<SchedulePeriodRange> resultingPeriods)
        {
            var today = DateTime.Today;
            var nowTime = DateTime.Now.TimeOfDay;
            var bookedStatus = AppointmentStatus.Booked.ToString();
            var confirmedStatus = AppointmentStatus.Confirmed.ToString();

            var futureAppointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId
                            && !a.IsDeleted
                            && (a.Status == bookedStatus || a.Status == confirmedStatus)
                            && (a.AppointmentDate > today || (a.AppointmentDate == today && a.AppointmentTime >= nowTime)))
                .Select(a => new { a.AppointmentDate, a.AppointmentTime })
                .ToListAsync();

            var appointmentsOnDay = futureAppointments
                .Where(a => (byte)a.AppointmentDate.DayOfWeek == dayOfWeek)
                .ToList();

            if (!appointmentsOnDay.Any())
                return null;

            var validSlots = new HashSet<TimeSpan>();
            if (resultingPeriods != null && resultingPeriods.Any() && slotDuration > 0)
            {
                var slotDurationSpan = TimeSpan.FromMinutes(slotDuration);
                foreach (var period in resultingPeriods)
                {
                    var currentTime = period.StartTime;
                    while (currentTime.Add(slotDurationSpan) <= period.EndTime)
                    {
                        validSlots.Add(currentTime);
                        currentTime = currentTime.Add(slotDurationSpan);
                    }
                }
            }

            var conflicting = appointmentsOnDay
                .Where(a => !validSlots.Contains(a.AppointmentTime))
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            if (conflicting.Any())
            {
                var first = conflicting.First();
                return $"schedule.AppointmentsConflict::{conflicting.Count}::{first.AppointmentDate:yyyy-MM-dd}::{first.AppointmentTime:hh\\:mm}";
            }

            return null;
        }

        private class SchedulePeriodRange
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }

            public SchedulePeriodRange(TimeSpan startTime, TimeSpan endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }
        }

        #endregion
    }
}
