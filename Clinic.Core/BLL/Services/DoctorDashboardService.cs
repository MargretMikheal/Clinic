using Clinic.Core.BLL.Interfaces;
using Clinic.Core.BLL.Dashboard.DTOs;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Services
{
    public class DoctorDashboardService : IDoctorDashboardService
    {
        public async Task<DoctorDashboardResponse> GetDashboardAsync(string userId)
        {
            using (var db = new Entities())
            {
                var doctor = await db.Doctors
                    .FirstOrDefaultAsync(d =>
                        !d.IsDeleted &&
                        d.UserId == userId);

                if (doctor == null)
                {
                    return new DoctorDashboardResponse();
                }

                var doctorId = doctor.Id;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var todayAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.DoctorId == doctorId && a.AppointmentDate == today)
                    .Select(a => new AppointmentDashboardItemDto
                    {
                        Id = a.Id,
                        PatientName = a.Patient.Name,
                        DoctorName = a.Doctor.Name,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status
                    })
                    .ToListAsync();

                var completedVisitsList = await db.Visits
                    .Where(v => !v.IsDeleted && v.DoctorId == doctorId && v.VisitDate >= today && v.VisitDate < tomorrow)
                    .Select(v => new VisitDashboardItemDto
                    {
                        Id = v.Id,
                        PatientName = v.Patient.Name,
                        DoctorName = v.Doctor.Name,
                        VisitDate = v.VisitDate,
                        Notes = v.Notes
                    })
                    .ToListAsync();

                var cancelledStatus = Clinic.Core.Enums.AppointmentStatus.Cancelled.ToString();
                var completedStatus = Clinic.Core.Enums.AppointmentStatus.Completed.ToString();

                var remainingAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.DoctorId == doctorId && a.AppointmentDate == today && a.Status != cancelledStatus && a.Status != completedStatus && !db.Visits.Any(v => !v.IsDeleted && v.AppointmentId == a.Id))
                    .Select(a => new AppointmentDashboardItemDto
                    {
                        Id = a.Id,
                        PatientName = a.Patient.Name,
                        DoctorName = a.Doctor.Name,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status
                    })
                    .ToListAsync();

                // Today's schedule
                var todayDayOfWeek = (byte)today.DayOfWeek;
                var todaySchedule = await db.DoctorSchedules
                    .Where(s =>
                        !s.IsDeleted &&
                        s.DoctorId == doctorId &&
                        s.DayOfWeek == todayDayOfWeek)
                    .Select(s => new ScheduleDashboardItemDto
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    })
                    .ToListAsync();

                // Recent patients (patients who had visits with this doctor)
                var recentPatients = await db.Visits
                    .Where(v =>
                        !v.IsDeleted &&
                        v.DoctorId == doctorId)
                    .OrderByDescending(v => v.VisitDate)
                    .Select(v => v.Patient)
                    .Where(p => !p.IsDeleted)
                    .Distinct()
                    .Take(10)
                    .Select(p => new PatientDashboardItemDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Phone = p.Phone,
                        Email = p.Email,
                        Gender = p.Gender
                    })
                    .ToListAsync();

                return new DoctorDashboardResponse
                {
                    Statistics = new List<DashboardStatisticDto>
                    {
                        new DashboardStatisticDto { Key = "TodayAppointments", Count = todayAppointmentsList.Count },
                        new DashboardStatisticDto { Key = "CompletedAppointments", Count = completedVisitsList.Count },
                        new DashboardStatisticDto { Key = "RemainingAppointments", Count = remainingAppointmentsList.Count }
                    },
                    TodaySchedule = todaySchedule,
                    RecentPatients = recentPatients,
                    TodayAppointments = todayAppointmentsList,
                    CompletedAppointments = completedVisitsList,
                    RemainingAppointments = remainingAppointmentsList
                };
            }
        }
    }
}
