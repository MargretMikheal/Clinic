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
    public class AdminDashboardService : IAdminDashboardService
    {
        public async Task<AdminDashboardResponse> GetDashboardAsync()
        {
            using (var db = new Entities())
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var currentLang = Clinic.Core.Common.LanguageHelper.GetCurrentLanguage();

                var doctors = await db.Doctors
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Select(d => new DoctorDashboardItemDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Speciality = currentLang == "en" ? d.Speciality.NameEn : d.Speciality.NameAr,
                        Phone = d.Phone,
                        Email = d.Email
                    })
                    .ToListAsync();

                var patients = await db.Patients
                    .Where(p => !p.IsDeleted)
                    .Select(p => new PatientDashboardItemDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Phone = p.Phone,
                        Email = p.Email,
                        Gender = p.Gender
                    })
                    .ToListAsync();

                var specialities = await db.Specialities
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new SpecialityDashboardItemDto
                    {
                        Id = s.Id,
                        Name = currentLang == "en" ? s.NameEn : s.NameAr,
                        DoctorCount = s.Doctors.Count(d => !d.IsDeleted && d.IsActive)
                    })
                    .ToListAsync();

                var todayAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.AppointmentDate == today)
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

                var todayCompletedVisitsList = await db.Visits
                    .Where(v => !v.IsDeleted && v.VisitDate >= today && v.VisitDate < tomorrow)
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

                var todayCancelledAppointmentsList = await db.Appointments
                    .Where(a => !a.IsDeleted && a.AppointmentDate == today && a.Status == cancelledStatus)
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

                var response = new AdminDashboardResponse
                {
                    Statistics = new List<DashboardStatisticDto>
                    {
                        new DashboardStatisticDto { Key = "Doctors", Count = doctors.Count },
                        new DashboardStatisticDto { Key = "Patients", Count = patients.Count },
                        new DashboardStatisticDto { Key = "Specialities", Count = specialities.Count },
                        new DashboardStatisticDto { Key = "TodayAppointments", Count = todayAppointmentsList.Count },
                        new DashboardStatisticDto { Key = "TodayCompletedVisits", Count = todayCompletedVisitsList.Count },
                        new DashboardStatisticDto { Key = "TodayCancelledAppointments", Count = todayCancelledAppointmentsList.Count }
                    },
                    Doctors = doctors,
                    Patients = patients,
                    Specialities = specialities,
                    TodayAppointments = todayAppointmentsList,
                    TodayCompletedVisits = todayCompletedVisitsList,
                    TodayCancelledAppointments = todayCancelledAppointmentsList
                };

                return response;
            }
        }
    }
}
