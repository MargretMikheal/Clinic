using Clinic.Core.BLL.Interfaces;
using Clinic.Core.Common.DTOs.Dashboard;
using Clinic.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Services
{
    public class ReceptionistDashboardService : IReceptionistDashboardService
    {
        public async Task<ReceptionistDashboardResponse> GetDashboardAsync(string userId)
        {
            using (var db = new Entities())
            {
                Guid receptionistGuid;
                if (!Guid.TryParse(userId, out receptionistGuid))
                {
                    return new ReceptionistDashboardResponse
                    {
                        DoctorName = string.Empty
                    };
                }

                var doctor = await db.Doctors
                    .FirstOrDefaultAsync(d =>
                        !d.IsDeleted &&
                        d.ReceptionistUserId.HasValue &&
                        d.ReceptionistUserId.Value == receptionistGuid);

                if (doctor == null)
                {
                    return new ReceptionistDashboardResponse
                    {
                        DoctorName = string.Empty
                    };
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

                var confirmedAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.DoctorId == doctorId && a.AppointmentDate == today && a.Status == "Confirmed")
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

                var completedAppointmentsList = await db.Visits
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

                var remainingAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.DoctorId == doctorId && a.AppointmentDate == today && a.Status != "Cancelled" && a.Status != "Completed" && !db.Visits.Any(v => !v.IsDeleted && v.AppointmentId == a.Id))
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

                return new ReceptionistDashboardResponse
                {
                    DoctorName = doctor.Name,
                    Statistics = new List<DashboardStatisticDto>
                    {
                        new DashboardStatisticDto { Key = "TodayAppointments", Count = todayAppointmentsList.Count },
                        new DashboardStatisticDto { Key = "ConfirmedAppointments", Count = confirmedAppointmentsList.Count },
                        new DashboardStatisticDto { Key = "CompletedAppointments", Count = completedAppointmentsList.Count },
                        new DashboardStatisticDto { Key = "RemainingAppointments", Count = remainingAppointmentsList.Count }
                    },
                    TodayAppointments = todayAppointmentsList,
                    ConfirmedAppointments = confirmedAppointmentsList,
                    CompletedAppointments = completedAppointmentsList,
                    RemainingAppointments = remainingAppointmentsList
                };
            }
        }
    }
}
