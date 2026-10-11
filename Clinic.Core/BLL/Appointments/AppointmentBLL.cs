using Clinic.Core.BLL.Appointments.DTOs;
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
    public class AppointmentBLL
    {
        private readonly Entities _context;

        public AppointmentBLL()
        {
            _context = new Entities();
        }

        #region Private Helper Methods

        private async Task<List<int>> GetAllowedDoctorIdsAsync(string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            if (isAdmin)
            {
                return null;
            }
            else if (isReceptionist)
            {
                Guid parsedId;
                if (Guid.TryParse(currentUserId, out parsedId))
                {
                    return await _context.Doctors
                        .Where(d => d.ReceptionistUserId == parsedId && !d.IsDeleted)
                        .Select(d => d.Id)
                        .ToListAsync();
                }
                return new List<int>();
            }
            else if (isDoctor)
            {
                return await _context.Doctors
                    .Where(d => d.UserId == currentUserId && !d.IsDeleted)
                    .Select(d => d.Id)
                    .ToListAsync();
            }
            return new List<int>();
        }

        private const string SystemUser = "System";
        private const string AppointmentExpiredMessage = "appointment.AppointmentExpired";

        private static bool IsExpired(Appointment appointment, DateTime now)
        {
            return appointment.AppointmentDate.Date + appointment.AppointmentTime <= now;
        }

        private async Task<bool> PatientExistsAsync(int patientId)
        {
            return await _context.Patients.AnyAsync(p => p.Id == patientId && !p.IsDeleted);
        }

        private async Task<Doctor> GetActiveDoctorAsync(int doctorId)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted && d.IsActive);
        }

        private async Task<bool> IsAppointmentTimeTakenAsync(int doctorId, DateTime appointmentDate, TimeSpan appointmentTime, int? excludedAppointmentId = null)
        {
            var targetDate = appointmentDate.Date;
            var cancelledStatus = AppointmentStatus.Cancelled.ToString();

            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate == targetDate &&
                            a.AppointmentTime == appointmentTime &&
                            !a.IsDeleted &&
                            a.Status != cancelledStatus);

            if (excludedAppointmentId.HasValue)
            {
                var id = excludedAppointmentId.Value;
                query = query.Where(a => a.Id != id);
            }

            return await query.AnyAsync();
        }

        private static IEnumerable<TimeSpan> GenerateSlots(IEnumerable<DoctorSchedule> schedules, int slotDuration)
        {
            var slotDurationSpan = TimeSpan.FromMinutes(slotDuration);
            foreach (var schedule in schedules)
            {
                var currentTime = schedule.StartTime;
                while (currentTime.Add(slotDurationSpan) <= schedule.EndTime)
                {
                    yield return currentTime;
                    currentTime = currentTime.Add(slotDurationSpan);
                }
            }
        }

        private async Task<bool> IsValidAppointmentSlotAsync(int doctorId, DateTime appointmentDate, TimeSpan appointmentTime, int slotDuration)
        {
            byte dayOfWeek = (byte)appointmentDate.DayOfWeek;
            var schedules = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek && !s.IsDeleted)
                .ToListAsync();

            var slots = GenerateSlots(schedules, slotDuration);
            foreach (var slot in slots)
            {
                if (slot == appointmentTime)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Public Methods
        public async Task<BaseResponse<List<AppointmentDoctorDto>>> GetAppointmentDoctorsAsync(string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);

                var query = _context.Doctors.Where(d => !d.IsDeleted && d.IsActive);

                if (!isAdmin)
                {
                    if (allowedDoctorIds == null || allowedDoctorIds.Count == 0)
                        return BaseResponse<List<AppointmentDoctorDto>>.Success(new List<AppointmentDoctorDto>());

                    query = query.Where(d => allowedDoctorIds.Contains(d.Id));
                }

                var doctors = await query
                    .Select(d => new AppointmentDoctorDto
                    {
                        Id = d.Id,
                        Name = d.Name
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return BaseResponse<List<AppointmentDoctorDto>>.Success(doctors);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<AppointmentDoctorDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<List<AppointmentListDto>>> GetAppointmentsAsync(
            string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor,
            DateTime? fromDate, DateTime? toDate, int? doctorId, string status, string searchText)
        {
            try
            {
                _context.usp_Appointment_MarkExpiredAsNoShow();

                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || allowedDoctorIds.Count == 0))
                    return BaseResponse<List<AppointmentListDto>>.Success(new List<AppointmentListDto>());

                var query = _context.Appointments
                    .Where(a => !a.IsDeleted);

                if (!isAdmin)
                {
                    query = query.Where(a => allowedDoctorIds.Contains(a.DoctorId));
                }

                if (doctorId.HasValue)
                {
                    query = query.Where(a => a.DoctorId == doctorId.Value);
                }

                if (fromDate.HasValue)
                {
                    var targetFromDate = fromDate.Value.Date;
                    query = query.Where(a => a.AppointmentDate >= targetFromDate);
                }

                if (toDate.HasValue)
                {
                    var targetToDate = toDate.Value.Date;
                    query = query.Where(a => a.AppointmentDate <= targetToDate);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(a => a.Patient.Name.Contains(searchText) ||
                                             a.Patient.PatientCode.Contains(searchText) ||
                                             a.Patient.Phone.Contains(searchText));
                }

                var results = await query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Select(a => new AppointmentListDto
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        PatientCode = a.Patient.PatientCode,
                        PatientName = a.Patient.Name,
                        PatientPhone = a.Patient.Phone,
                        DoctorId = a.DoctorId,
                        DoctorName = a.Doctor.Name,
                        DoctorCode = a.Doctor.DoctorCode,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status,
                        Notes = a.Notes
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return BaseResponse<List<AppointmentListDto>>.Success(results);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<AppointmentListDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<AppointmentDto>> GetAppointmentByIdAsync(
            int id, string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                var dto = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.Id == id && !a.IsDeleted)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        PatientName = a.Patient.Name,
                        DoctorId = a.DoctorId,
                        DoctorName = a.Doctor.Name,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status,
                        Notes = a.Notes
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                    return BaseResponse<AppointmentDto>.Fail("Appointment not found.");

                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(dto.DoctorId)))
                {
                    return BaseResponse<AppointmentDto>.Fail("You are not allowed to access this doctor's appointments.");
                }

                return BaseResponse<AppointmentDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return BaseResponse<AppointmentDto>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<List<AvailableSlotDto>>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                var doctor = await GetActiveDoctorAsync(doctorId);
                if (doctor == null)
                    return BaseResponse<List<AvailableSlotDto>>.Fail("Doctor not found.");
                if (!doctor.IsActive)
                    return BaseResponse<List<AvailableSlotDto>>.Fail("Doctor is not active.");
                if (doctor.SlotDuration <= 0)
                    return BaseResponse<List<AvailableSlotDto>>.Fail("Doctor has invalid slot duration.");

                byte dayOfWeek = (byte)date.DayOfWeek;

                var schedules = await _context.DoctorSchedules
                    .AsNoTracking()
                    .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek && !s.IsDeleted)
                    .ToListAsync();

                if (!schedules.Any())
                    return BaseResponse<List<AvailableSlotDto>>.Success(new List<AvailableSlotDto>());

                var generatedSlots = new HashSet<TimeSpan>(GenerateSlots(schedules, doctor.SlotDuration));

                var targetDate = date.Date;
                var cancelledStatus = AppointmentStatus.Cancelled.ToString();
                var existingAppointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && a.AppointmentDate == targetDate && !a.IsDeleted && a.Status != cancelledStatus)
                    .Select(a => a.AppointmentTime)
                    .ToListAsync();

                generatedSlots.ExceptWith(existingAppointments);

                if (targetDate == DateTime.Today)
                {
                    var nowTime = DateTime.Now.TimeOfDay;
                    generatedSlots.RemoveWhere(s => s <= nowTime);
                }

                var slots = generatedSlots
                    .OrderBy(s => s)
                    .Select(s => new AvailableSlotDto { Time = s.ToString(@"hh\:mm") })
                    .ToList();

                return BaseResponse<List<AvailableSlotDto>>.Success(slots);
            }
            catch (Exception ex)
            {
                return BaseResponse<List<AvailableSlotDto>>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> CreateAppointmentAsync(
            CreateAppointmentDto dto, string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(dto.DoctorId)))
                {
                    return BaseResponse<bool>.Fail("You are not allowed to access this doctor's appointments.");
                }

                if (!await PatientExistsAsync(dto.PatientId))
                    return BaseResponse<bool>.Fail("Patient not found.");

                var doctor = await GetActiveDoctorAsync(dto.DoctorId);
                if (doctor == null)
                    return BaseResponse<bool>.Fail("Doctor not found.");
                if (!doctor.IsActive)
                    return BaseResponse<bool>.Fail("Doctor is not active.");

                if (dto.AppointmentDate.Date < DateTime.Today)
                    return BaseResponse<bool>.Fail("Appointment date cannot be in the past.");

                if (!await IsValidAppointmentSlotAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime, doctor.SlotDuration))
                    return BaseResponse<bool>.Fail("Selected time is outside the doctor's schedule or not a valid slot.");

                if (dto.AppointmentDate.Date == DateTime.Today && dto.AppointmentTime <= DateTime.Now.TimeOfDay)
                {
                    return BaseResponse<bool>.Fail("Selected time has already passed.");
                }

                if (await IsAppointmentTimeTakenAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime))
                    return BaseResponse<bool>.Fail("Selected time is no longer available.");

                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    AppointmentDate = dto.AppointmentDate.Date,
                    AppointmentTime = dto.AppointmentTime,
                    Status = AppointmentStatus.Booked.ToString(),
                    Notes = dto.Notes,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId,
                    IsDeleted = false
                };

                _context.Appointments.Add(appointment);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    return BaseResponse<bool>.Fail("Selected time is no longer available.");
                }

                return BaseResponse<bool>.Success(true, "Appointment created successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> UpdateAppointmentAsync(
            UpdateAppointmentDto dto, string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == dto.Id && !a.IsDeleted);

                if (appointment == null)
                    return BaseResponse<bool>.Fail("Appointment not found.");

                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(appointment.DoctorId)))
                {
                    return BaseResponse<bool>.Fail("You are not allowed to access this doctor's appointments.");
                }

                if (appointment.DoctorId != dto.DoctorId)
                {
                    if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(dto.DoctorId)))
                    {
                        return BaseResponse<bool>.Fail("You are not allowed to assign appointments to this doctor.");
                    }
                }

                var completedStatus = AppointmentStatus.Completed.ToString();
                if (appointment.Status == completedStatus)
                    return BaseResponse<bool>.Fail("Appointment cannot be modified after completion.");

                var cancelledStatus = AppointmentStatus.Cancelled.ToString();
                if (appointment.Status == cancelledStatus)
                    return BaseResponse<bool>.Fail("Cancelled appointments cannot be modified.");

                var now = DateTime.Now;
                if (IsExpired(appointment, now))
                {
                    if (appointment.Status == AppointmentStatus.Booked.ToString() ||
                        appointment.Status == AppointmentStatus.Confirmed.ToString())
                    {
                        appointment.Status = AppointmentStatus.NoShow.ToString();
                        appointment.ModifiedDate = now;
                        appointment.ModifiedBy = SystemUser;
                    }

                    await _context.SaveChangesAsync();
                    return BaseResponse<bool>.Fail(AppointmentExpiredMessage);
                }

                if (appointment.PatientId != dto.PatientId)
                {
                    if (!await PatientExistsAsync(dto.PatientId))
                        return BaseResponse<bool>.Fail("Patient not found.");
                }

                if (appointment.DoctorId != dto.DoctorId)
                {
                    var checkDoctor = await GetActiveDoctorAsync(dto.DoctorId);
                    if (checkDoctor == null)
                        return BaseResponse<bool>.Fail("Doctor not found.");
                    if (!checkDoctor.IsActive)
                        return BaseResponse<bool>.Fail("Doctor is not active.");
                }

                if (appointment.DoctorId != dto.DoctorId ||
                    appointment.AppointmentDate.Date != dto.AppointmentDate.Date ||
                    appointment.AppointmentTime != dto.AppointmentTime)
                {
                    if (dto.AppointmentDate.Date < DateTime.Today)
                        return BaseResponse<bool>.Fail("Appointment date cannot be in the past.");

                    var doctor = await GetActiveDoctorAsync(dto.DoctorId);
                    if (doctor == null) return BaseResponse<bool>.Fail("Doctor not found.");

                    if (!await IsValidAppointmentSlotAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime, doctor.SlotDuration))
                        return BaseResponse<bool>.Fail("Selected time is outside the doctor's schedule or not a valid slot.");

                    if (dto.AppointmentDate.Date == DateTime.Today && dto.AppointmentTime <= DateTime.Now.TimeOfDay)
                    {
                        return BaseResponse<bool>.Fail("Selected time has already passed.");
                    }

                    if (await IsAppointmentTimeTakenAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime, appointment.Id))
                        return BaseResponse<bool>.Fail("Selected time is no longer available.");
                }

                appointment.PatientId = dto.PatientId;
                appointment.DoctorId = dto.DoctorId;
                appointment.AppointmentDate = dto.AppointmentDate.Date;
                appointment.AppointmentTime = dto.AppointmentTime;
                appointment.Notes = dto.Notes;
                appointment.ModifiedDate = DateTime.Now;
                appointment.ModifiedBy = currentUserId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException)
                {
                    return BaseResponse<bool>.Fail("Selected time is no longer available.");
                }

                return BaseResponse<bool>.Success(true, "Appointment updated successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> ChangeAppointmentStatusAsync(ChangeAppointmentStatusDto dto, string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                if (dto == null)
                    return BaseResponse<bool>.Fail("Invalid request.");

                var validStatuses = new[]
                {
                    AppointmentStatus.Booked.ToString(),
                    AppointmentStatus.Confirmed.ToString(),
                    AppointmentStatus.Completed.ToString(),
                    AppointmentStatus.Cancelled.ToString(),
                    AppointmentStatus.NoShow.ToString()
                };

                if (!validStatuses.Contains(dto.Status))
                    return BaseResponse<bool>.Fail("appointment.InvalidAppointmentStatus");

                var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && !a.IsDeleted);
                if (appointment == null)
                    return BaseResponse<bool>.Fail("Appointment not found.");

                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(appointment.DoctorId)))
                {
                    return BaseResponse<bool>.Fail("You are not allowed to access this doctor's appointments.");
                }

                var completedStatus = AppointmentStatus.Completed.ToString();
                if (appointment.Status == completedStatus)
                {
                    return BaseResponse<bool>.Fail("appointment.AppointmentCannotChangeStatus");
                }

                var now = DateTime.Now;
                if (IsExpired(appointment, now))
                {
                    if (appointment.Status == AppointmentStatus.Booked.ToString() ||
                        appointment.Status == AppointmentStatus.Confirmed.ToString())
                    {
                        appointment.Status = AppointmentStatus.NoShow.ToString();
                        appointment.ModifiedDate = now;
                        appointment.ModifiedBy = SystemUser;
                    }

                    await _context.SaveChangesAsync();
                    return BaseResponse<bool>.Fail(AppointmentExpiredMessage);
                }

                appointment.Status = dto.Status;
                appointment.ModifiedDate = DateTime.Now;
                appointment.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(true, "appointment.AppointmentStatusUpdatedSuccess");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }

        public async Task<BaseResponse<bool>> DeleteAppointmentAsync(
            int id, string currentUserId, bool isAdmin, bool isReceptionist, bool isDoctor)
        {
            try
            {
                var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
                if (appointment == null)
                    return BaseResponse<bool>.Fail("Appointment not found.");

                var allowedDoctorIds = await GetAllowedDoctorIdsAsync(currentUserId, isAdmin, isReceptionist, isDoctor);
                if (!isAdmin && (allowedDoctorIds == null || !allowedDoctorIds.Contains(appointment.DoctorId)))
                {
                    return BaseResponse<bool>.Fail("You are not allowed to access this doctor's appointments.");
                }

                var completedStatus = AppointmentStatus.Completed.ToString();
                if (appointment.Status == completedStatus)
                    return BaseResponse<bool>.Fail("Completed appointments cannot be deleted.");

                appointment.IsDeleted = true;
                appointment.ModifiedDate = DateTime.Now;
                appointment.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync();

                return BaseResponse<bool>.Success(true, "Appointment deleted successfully.");
            }
            catch (Exception ex)
            {
                return BaseResponse<bool>.Fail(ex.Message);
            }
        }
        #endregion
    }
}

