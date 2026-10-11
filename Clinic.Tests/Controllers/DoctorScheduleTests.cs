using Clinic.Core.BLL;
using Clinic.Core.BLL.Doctors.DTOs;
using Clinic.Core.Constants;
using Clinic.Core.Data;
using Clinic.Core.Enums;
using Clinic.Web.Areas.Doctor.Controllers;
using Clinic.Web.Infrastructure.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Tests.Controllers
{
    [TestClass]
    public class DoctorScheduleTests
    {
        private DoctorsBLL _bll;
        private Entities _db;
        private string _testDoctorUserId;
        private int _testDoctorId;
        private string _otherDoctorUserId;
        private int _otherDoctorId;

        [TestInitialize]
        public void Setup()
        {
            _bll = new DoctorsBLL();
            _db = new Entities();

            // Find two active doctors from DB for testing
            var doctors = _db.Doctors.Where(d => !d.IsDeleted && d.IsActive && !string.IsNullOrEmpty(d.UserId)).Take(2).ToList();
            Assert.IsTrue(doctors.Count >= 2, "Test requires at least two active doctors in database.");

            _testDoctorId = doctors[0].Id;
            _testDoctorUserId = doctors[0].UserId;

            _otherDoctorId = doctors[1].Id;
            _otherDoctorUserId = doctors[1].UserId;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _db?.Dispose();
        }

        #region Authorization & Attribute Verification

        [TestMethod]
        public void ScheduleController_ShouldHaveAuthorizeAttribute()
        {
            var controllerType = typeof(ScheduleController);
            var authorizeAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();
            Assert.IsNotNull(authorizeAttr, "ScheduleController must be decorated with [Authorize]");
        }

        [TestMethod]
        public void ScheduleController_ActionPermissions_ShouldMatchConventions()
        {
            var controllerType = typeof(ScheduleController);

            // Index
            var indexMethod = controllerType.GetMethod("Index");
            var indexAttr = indexMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(indexAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, indexAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Show, indexAttr.RequiredPermission);

            // GetMySchedule
            var getMethod = controllerType.GetMethod("GetMySchedule");
            var getAttr = getMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(getAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, getAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Show, getAttr.RequiredPermission);

            // AddPeriod
            var addMethod = controllerType.GetMethod("AddPeriod");
            var addAttr = addMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(addAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, addAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Add, addAttr.RequiredPermission);

            // UpdatePeriod
            var updateMethod = controllerType.GetMethod("UpdatePeriod");
            var updateAttr = updateMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(updateAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, updateAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Edit, updateAttr.RequiredPermission);

            // DeletePeriod
            var deleteMethod = controllerType.GetMethod("DeletePeriod");
            var deleteAttr = deleteMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(deleteAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, deleteAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Delete, deleteAttr.RequiredPermission);

            // GetActiveDoctors
            var getActiveDocsMethod = controllerType.GetMethod("GetActiveDoctors");
            var getActiveDocsAttr = getActiveDocsMethod.GetCustomAttribute<ScreenAuthorizeAttribute>();
            Assert.IsNotNull(getActiveDocsAttr);
            Assert.AreEqual(ScreenKeys.MySchedule, getActiveDocsAttr.ScreenId);
            Assert.AreEqual(ScreenPermission.Show, getActiveDocsAttr.RequiredPermission);
        }

        #endregion

        #region Get Schedule Tests

        [TestMethod]
        public async Task GetMyWeeklySchedule_ValidDoctor_ShouldReturnSchedulesAndSlotDuration()
        {
            var response = await _bll.GetMyWeeklyScheduleAsync(_testDoctorUserId);

            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_testDoctorId, response.Data.DoctorId);
            Assert.IsTrue(response.Data.SlotDuration > 0);
            Assert.IsNotNull(response.Data.Schedules);
        }

        [TestMethod]
        public async Task GetMyWeeklySchedule_NonExistentUser_ShouldFail()
        {
            var response = await _bll.GetMyWeeklyScheduleAsync(Guid.NewGuid().ToString());

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.DoctorNotFound", response.Message);
        }

        [TestMethod]
        public async Task GetMyWeeklySchedule_NullUser_ShouldFail()
        {
            var response = await _bll.GetMyWeeklyScheduleAsync(null);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.Unauthorized", response.Message);
        }

        #endregion

        #region Add Period Validation Tests

        [TestMethod]
        public async Task AddSchedulePeriod_ValidPeriod_ShouldPersistCorrectly()
        {
            // Pick a day with no periods or a non-overlapping time slot
            byte testDay = 5; // Friday
            var dto = new DoctorScheduleDto
            {
                DayOfWeek = testDay,
                StartTime = "06:00",
                EndTime = "07:00"
            };

            // Ensure no existing period at 06:00-07:00 on Friday
            var existing = _db.DoctorSchedules
                .Where(s => s.DoctorId == _testDoctorId && s.DayOfWeek == testDay && !s.IsDeleted && s.StartTime < new TimeSpan(7, 0, 0) && new TimeSpan(6, 0, 0) < s.EndTime)
                .ToList();
            foreach (var ex in existing) { ex.IsDeleted = true; }
            _db.SaveChanges();

            var response = await _bll.AddSchedulePeriodAsync(dto, _testDoctorUserId);

            Assert.IsTrue(response.IsSuccess, response.Message);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue(response.Data.Id > 0);

            // Verify in DB
            using (var verifyDb = new Entities())
            {
                var created = verifyDb.DoctorSchedules.Find(response.Data.Id);
                Assert.IsNotNull(created);
                Assert.AreEqual(_testDoctorId, created.DoctorId);
                Assert.AreEqual(testDay, created.DayOfWeek);
                Assert.AreEqual(new TimeSpan(6, 0, 0), created.StartTime);
                Assert.AreEqual(new TimeSpan(7, 0, 0), created.EndTime);
                Assert.IsFalse(created.IsDeleted);
                Assert.AreEqual(_testDoctorUserId, created.CreatedBy);

                // Cleanup
                created.IsDeleted = true;
                verifyDb.SaveChanges();
            }
        }

        [TestMethod]
        public async Task AddSchedulePeriod_StartTimeAfterEndTime_ShouldBeRejected()
        {
            var dto = new DoctorScheduleDto
            {
                DayOfWeek = 1,
                StartTime = "17:00",
                EndTime = "09:00"
            };

            var response = await _bll.AddSchedulePeriodAsync(dto, _testDoctorUserId);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.InvalidTimeRange", response.Message);
        }

        [TestMethod]
        public async Task AddSchedulePeriod_EqualStartTimeAndEndTime_ShouldBeRejected()
        {
            var dto = new DoctorScheduleDto
            {
                DayOfWeek = 1,
                StartTime = "10:00",
                EndTime = "10:00"
            };

            var response = await _bll.AddSchedulePeriodAsync(dto, _testDoctorUserId);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.InvalidTimeRange", response.Message);
        }

        [TestMethod]
        public async Task AddSchedulePeriod_ShorterThanSlotDuration_ShouldBeRejected()
        {
            var doctor = _db.Doctors.Find(_testDoctorId);
            var slotDuration = doctor.SlotDuration;

            if (slotDuration > 5)
            {
                // Create a period shorter than slotDuration (e.g. 5 minutes)
                var dto = new DoctorScheduleDto
                {
                    DayOfWeek = 2,
                    StartTime = "08:00",
                    EndTime = "08:05"
                };

                var response = await _bll.AddSchedulePeriodAsync(dto, _testDoctorUserId);

                Assert.IsFalse(response.IsSuccess);
                Assert.IsTrue(response.Message.StartsWith("schedule.PeriodShorterThanSlot"));
            }
        }

        [TestMethod]
        public async Task AddSchedulePeriod_OverlappingPeriod_ShouldBeRejected()
        {
            byte testDay = 6; // Saturday
            var first = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
            {
                DayOfWeek = testDay,
                StartTime = "08:00",
                EndTime = "12:00"
            }, _testDoctorUserId);

            Assert.IsTrue(first.IsSuccess);

            try
            {
                // Attempt overlapping: 10:00 - 14:00
                var overlapDto = new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "10:00",
                    EndTime = "14:00"
                };

                var response = await _bll.AddSchedulePeriodAsync(overlapDto, _testDoctorUserId);

                Assert.IsFalse(response.IsSuccess);
                Assert.AreEqual("schedule.OverlappingPeriod", response.Message);
            }
            finally
            {
                // Clean up created schedule
                using (var cleanupDb = new Entities())
                {
                    var s = cleanupDb.DoctorSchedules.Find(first.Data.Id);
                    if (s != null)
                    {
                        s.IsDeleted = true;
                        cleanupDb.SaveChanges();
                    }
                }
            }
        }

        #endregion

        #region Ownership Enforcement Tests

        [TestMethod]
        public async Task UpdateSchedulePeriod_BelongsToAnotherDoctor_ShouldBeRejected()
        {
            // Find or create a schedule belonging to _otherDoctorId
            var otherSchedule = _db.DoctorSchedules.FirstOrDefault(s => s.DoctorId == _otherDoctorId && !s.IsDeleted);
            if (otherSchedule == null)
            {
                otherSchedule = _db.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = _otherDoctorId,
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    CreatedDate = DateTime.Now,
                    CreatedBy = _otherDoctorUserId,
                    IsDeleted = false
                });
                _db.SaveChanges();
            }

            var dto = new DoctorScheduleDto
            {
                Id = otherSchedule.Id,
                DayOfWeek = otherSchedule.DayOfWeek,
                StartTime = "08:00",
                EndTime = "13:00"
            };

            // TestDoctor attempts to update OtherDoctor's schedule!
            var response = await _bll.UpdateSchedulePeriodAsync(dto, _testDoctorUserId);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.Unauthorized", response.Message);
        }

        [TestMethod]
        public async Task DeleteSchedulePeriod_BelongsToAnotherDoctor_ShouldBeRejected()
        {
            var otherSchedule = _db.DoctorSchedules.FirstOrDefault(s => s.DoctorId == _otherDoctorId && !s.IsDeleted);
            if (otherSchedule == null)
            {
                otherSchedule = _db.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = _otherDoctorId,
                    DayOfWeek = 3,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    CreatedDate = DateTime.Now,
                    CreatedBy = _otherDoctorUserId,
                    IsDeleted = false
                });
                _db.SaveChanges();
            }

            // TestDoctor attempts to delete OtherDoctor's schedule!
            var response = await _bll.DeleteSchedulePeriodAsync(otherSchedule.Id, _testDoctorUserId);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual("schedule.Unauthorized", response.Message);
        }

        #endregion

        #region Appointment Protection Tests

        [TestMethod]
        public async Task DeletePeriod_WithFutureBookedAppointment_ShouldRejectAndProtectAppointment()
        {
            byte testDay = 4; // Thursday
            // Create a test schedule period for testDoctor
            var schedule = _db.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = _testDoctorId,
                DayOfWeek = testDay,
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                CreatedDate = DateTime.Now,
                CreatedBy = _testDoctorUserId,
                IsDeleted = false
            });
            _db.SaveChanges();

            // Find next date that matches testDay (Thursday)
            var nextDate = DateTime.Today.AddDays(1);
            while ((byte)nextDate.DayOfWeek != testDay)
            {
                nextDate = nextDate.AddDays(1);
            }

            // Create a patient if needed
            var patient = _db.Patients.FirstOrDefault(p => !p.IsDeleted);
            Assert.IsNotNull(patient, "Test requires at least one patient in DB.");

            // Create a future Booked appointment in that period (14:00)
            var appointment = _db.Appointments.Add(new Appointment
            {
                DoctorId = _testDoctorId,
                PatientId = patient.Id,
                AppointmentDate = nextDate,
                AppointmentTime = new TimeSpan(14, 0, 0),
                Status = AppointmentStatus.Booked.ToString(),
                CreatedDate = DateTime.Now,
                CreatedBy = _testDoctorUserId,
                IsDeleted = false
            });
            _db.SaveChanges();

            try
            {
                // Attempt to delete this schedule period which would invalidate the appointment!
                var response = await _bll.DeleteSchedulePeriodAsync(schedule.Id, _testDoctorUserId);

                Assert.IsFalse(response.IsSuccess, "Deleting schedule period with future appointments must be rejected.");
                Assert.IsTrue(response.Message.StartsWith("schedule.AppointmentsConflict"), $"Expected AppointmentsConflict message, got: {response.Message}");

                // Verify the appointment is still intact and not cancelled or deleted
                using (var checkDb = new Entities())
                {
                    var checkedApp = checkDb.Appointments.Find(appointment.Id);
                    Assert.IsNotNull(checkedApp);
                    Assert.AreEqual(AppointmentStatus.Booked.ToString(), checkedApp.Status);
                    Assert.IsFalse(checkedApp.IsDeleted);

                    var checkedSched = checkDb.DoctorSchedules.Find(schedule.Id);
                    Assert.IsNotNull(checkedSched);
                    Assert.IsFalse(checkedSched.IsDeleted, "Schedule must not be deleted when appointment conflict occurs.");
                }
            }
            finally
            {
                // Cleanup test data
                using (var cleanupDb = new Entities())
                {
                    var app = cleanupDb.Appointments.Find(appointment.Id);
                    if (app != null) cleanupDb.Appointments.Remove(app);

                    var s = cleanupDb.DoctorSchedules.Find(schedule.Id);
                    if (s != null) cleanupDb.DoctorSchedules.Remove(s);

                    cleanupDb.SaveChanges();
                }
            }
        }

        [TestMethod]
        public async Task UpdatePeriod_WhenShrunkExcludingFutureAppointment_ShouldRejectAndProtectAppointment()
        {
            byte testDay = 4; // Thursday
            // Create a test schedule period: 10:00 to 14:00
            var schedule = _db.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = _testDoctorId,
                DayOfWeek = testDay,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(14, 0, 0),
                CreatedDate = DateTime.Now,
                CreatedBy = _testDoctorUserId,
                IsDeleted = false
            });
            _db.SaveChanges();

            var nextDate = DateTime.Today.AddDays(1);
            while ((byte)nextDate.DayOfWeek != testDay)
            {
                nextDate = nextDate.AddDays(1);
            }

            var patient = _db.Patients.FirstOrDefault(p => !p.IsDeleted);
            Assert.IsNotNull(patient);

            // Future Confirmed appointment at 13:00
            var appointment = _db.Appointments.Add(new Appointment
            {
                DoctorId = _testDoctorId,
                PatientId = patient.Id,
                AppointmentDate = nextDate,
                AppointmentTime = new TimeSpan(13, 0, 0),
                Status = AppointmentStatus.Confirmed.ToString(),
                CreatedDate = DateTime.Now,
                CreatedBy = _testDoctorUserId,
                IsDeleted = false
            });
            _db.SaveChanges();

            try
            {
                // Shrink schedule to 10:00 - 12:00 (which excludes 13:00 appointment!)
                var dto = new DoctorScheduleDto
                {
                    Id = schedule.Id,
                    DayOfWeek = testDay,
                    StartTime = "10:00",
                    EndTime = "12:00"
                };

                var response = await _bll.UpdateSchedulePeriodAsync(dto, _testDoctorUserId);

                Assert.IsFalse(response.IsSuccess, "Shrinking schedule to exclude appointment must be rejected.");
                Assert.IsTrue(response.Message.StartsWith("schedule.AppointmentsConflict"), $"Expected AppointmentsConflict message, got: {response.Message}");

                // Verify appointment and schedule untouched
                using (var checkDb = new Entities())
                {
                    var checkedApp = checkDb.Appointments.Find(appointment.Id);
                    Assert.AreEqual(AppointmentStatus.Confirmed.ToString(), checkedApp.Status);


                    var checkedSched = checkDb.DoctorSchedules.Find(schedule.Id);
                    Assert.AreEqual(new TimeSpan(14, 0, 0), checkedSched.EndTime, "Schedule EndTime must remain unchanged.");
                }
            }
            finally
            {
                using (var cleanupDb = new Entities())
                {
                    var app = cleanupDb.Appointments.Find(appointment.Id);
                    if (app != null) cleanupDb.Appointments.Remove(app);

                    var s = cleanupDb.DoctorSchedules.Find(schedule.Id);
                    if (s != null) cleanupDb.DoctorSchedules.Remove(s);

                    cleanupDb.SaveChanges();
                }
            }
        }

        #endregion

        #region Multiple Periods and Soft-Delete Tests

        [TestMethod]
        public async Task AddMultiplePeriods_SameDay_NonOverlapping_ShouldSucceed()
        {
            byte testDay = 5; // Friday
            int id1 = 0, id2 = 0;

            try
            {
                var res1 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "07:00",
                    EndTime = "09:00"
                }, _testDoctorUserId);

                Assert.IsTrue(res1.IsSuccess, res1.Message);
                id1 = res1.Data.Id;

                var res2 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "18:00",
                    EndTime = "21:00"
                }, _testDoctorUserId);

                Assert.IsTrue(res2.IsSuccess, res2.Message);
                id2 = res2.Data.Id;

                // Load schedule and verify both periods are returned on the same day
                var scheduleRes = await _bll.GetMyWeeklyScheduleAsync(_testDoctorUserId);
                Assert.IsTrue(scheduleRes.IsSuccess);
                var fridayPeriods = scheduleRes.Data.Schedules.Where(s => s.DayOfWeek == testDay).ToList();
                Assert.IsTrue(fridayPeriods.Any(s => s.Id == id1));
                Assert.IsTrue(fridayPeriods.Any(s => s.Id == id2));
            }
            finally
            {
                using (var cleanupDb = new Entities())
                {
                    if (id1 > 0)
                    {
                        var s1 = cleanupDb.DoctorSchedules.Find(id1);
                        if (s1 != null) cleanupDb.DoctorSchedules.Remove(s1);
                    }
                    if (id2 > 0)
                    {
                        var s2 = cleanupDb.DoctorSchedules.Find(id2);
                        if (s2 != null) cleanupDb.DoctorSchedules.Remove(s2);
                    }
                    cleanupDb.SaveChanges();
                }
            }
        }

        [TestMethod]
        public async Task DeleteSchedulePeriod_ValidPeriodNoConflict_ShouldSoftDelete()
        {
            byte testDay = 5; // Friday
            var addRes = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
            {
                DayOfWeek = testDay,
                StartTime = "05:00",
                EndTime = "06:00"
            }, _testDoctorUserId);

            Assert.IsTrue(addRes.IsSuccess, addRes.Message);
            int periodId = addRes.Data.Id;

            try
            {
                var delRes = await _bll.DeleteSchedulePeriodAsync(periodId, _testDoctorUserId);
                Assert.IsTrue(delRes.IsSuccess, delRes.Message);

                // Verify soft-delete in DB
                using (var verifyDb = new Entities())
                {
                    var record = verifyDb.DoctorSchedules.Find(periodId);
                    Assert.IsNotNull(record);
                    Assert.IsTrue(record.IsDeleted, "Record must be marked as IsDeleted = true");
                    Assert.IsNotNull(record.ModifiedDate);
                    Assert.AreEqual(_testDoctorUserId, record.ModifiedBy);
                }

                // Verify that GetMyWeeklySchedule excludes soft-deleted period
                var schedRes = await _bll.GetMyWeeklyScheduleAsync(_testDoctorUserId);
                Assert.IsTrue(schedRes.IsSuccess);
                Assert.IsFalse(schedRes.Data.Schedules.Any(s => s.Id == periodId), "Deleted schedule must not appear in active schedule");
            }
            finally
            {
                using (var cleanupDb = new Entities())
                {
                    var record = cleanupDb.DoctorSchedules.Find(periodId);
                    if (record != null) cleanupDb.DoctorSchedules.Remove(record);
                    cleanupDb.SaveChanges();
                }
            }
        }

        [TestMethod]
        public async Task OverlappingChecks_VariousScenarios_ShouldBeRejected()
        {
            byte testDay = 6; // Saturday
            var baseRes = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
            {
                DayOfWeek = testDay,
                StartTime = "10:00",
                EndTime = "14:00"
            }, _testDoctorUserId);

            Assert.IsTrue(baseRes.IsSuccess, baseRes.Message);
            int baseId = baseRes.Data.Id;

            try
            {
                // 1. Encompassing: 09:00 - 15:00
                var r1 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "09:00",
                    EndTime = "15:00"
                }, _testDoctorUserId);
                Assert.IsFalse(r1.IsSuccess);
                Assert.AreEqual("schedule.OverlappingPeriod", r1.Message);

                // 2. Inside: 11:00 - 13:00
                var r2 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "11:00",
                    EndTime = "13:00"
                }, _testDoctorUserId);
                Assert.IsFalse(r2.IsSuccess);
                Assert.AreEqual("schedule.OverlappingPeriod", r2.Message);

                // 3. Partial overlap at start: 09:00 - 11:00
                var r3 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "09:00",
                    EndTime = "11:00"
                }, _testDoctorUserId);
                Assert.IsFalse(r3.IsSuccess);
                Assert.AreEqual("schedule.OverlappingPeriod", r3.Message);

                // 4. Partial overlap at end: 13:00 - 15:00
                var r4 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "13:00",
                    EndTime = "15:00"
                }, _testDoctorUserId);
                Assert.IsFalse(r4.IsSuccess);
                Assert.AreEqual("schedule.OverlappingPeriod", r4.Message);

                // 5. Adjacent period (touching endpoint): 14:00 - 16:00 SHOULD SUCCEED (no overlap)
                var r5 = await _bll.AddSchedulePeriodAsync(new DoctorScheduleDto
                {
                    DayOfWeek = testDay,
                    StartTime = "14:00",
                    EndTime = "16:00"
                }, _testDoctorUserId);
                Assert.IsTrue(r5.IsSuccess, "Adjacent periods that share boundary time must not be considered overlapping.");

                using (var cleanupDb = new Entities())
                {
                    var adj = cleanupDb.DoctorSchedules.Find(r5.Data.Id);
                    if (adj != null) cleanupDb.DoctorSchedules.Remove(adj);
                    cleanupDb.SaveChanges();
                }
            }
            finally
            {
                using (var cleanupDb = new Entities())
                {
                    var b = cleanupDb.DoctorSchedules.Find(baseId);
                    if (b != null) cleanupDb.DoctorSchedules.Remove(b);
                    cleanupDb.SaveChanges();
                }
            }
        }

        #endregion

        #region Admin Mode & Role Isolation Tests

        [TestMethod]
        public async Task Admin_GetActiveDoctors_ShouldReturnDoctorsList()
        {
            var response = await _bll.GetActiveDoctorsForScheduleAsync();
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue(response.Data.Count >= 2, "Should return at least the 2 active doctors.");
            Assert.IsTrue(response.Data.Any(d => d.Id == _testDoctorId));
            Assert.IsTrue(response.Data.Any(d => d.Id == _otherDoctorId));
        }

        [TestMethod]
        public async Task Admin_GetWeeklySchedule_WithoutDoctorId_ShouldReturnSuccessNull()
        {
            // Admin opened page, no doctor selected yet
            var response = await _bll.GetWeeklyScheduleAsync(_testDoctorUserId, true, null);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNull(response.Data, "When Admin has not selected a doctor, response data should be null.");
        }

        [TestMethod]
        public async Task Admin_GetWeeklySchedule_WithTargetDoctorId_ShouldReturnDoctorSchedule()
        {
            // Admin selected other doctor
            var response = await _bll.GetWeeklyScheduleAsync(_testDoctorUserId, true, _otherDoctorId);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_otherDoctorId, response.Data.DoctorId);
        }

        [TestMethod]
        public async Task Doctor_CannotAccessOtherDoctorSchedule_EvenIfTargetDoctorIdProvided()
        {
            // Normal doctor mode ignores targetDoctorId and resolves strictly by userId
            var response = await _bll.GetWeeklyScheduleAsync(_testDoctorUserId, false, _otherDoctorId);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_testDoctorId, response.Data.DoctorId, "Doctor mode must resolve to the authenticated doctor, never the targetDoctorId.");
        }

        [TestMethod]
        public async Task Admin_AddEditDeletePeriod_ForTargetDoctor_ShouldSucceed()
        {
            byte testDay = 5; // Friday
            var adminUserId = _testDoctorUserId; // Simulated admin user id
            int newPeriodId = 0;

            try
            {
                // 1. Admin Adds period for _otherDoctorId
                var addDto = new DoctorScheduleDto
                {
                    DoctorId = _otherDoctorId,
                    DayOfWeek = testDay,
                    StartTime = "06:00",
                    EndTime = "08:00"
                };

                var addResult = await _bll.AddSchedulePeriodAsync(addDto, adminUserId, true, _otherDoctorId);
                Assert.IsTrue(addResult.IsSuccess, $"Admin add period should succeed: {addResult.Message}");
                Assert.IsNotNull(addResult.Data);
                newPeriodId = addResult.Data.Id;
                Assert.AreEqual(_otherDoctorId, addResult.Data.DoctorId);

                // 2. Admin Updates period for _otherDoctorId
                var updateDto = new DoctorScheduleDto
                {
                    Id = newPeriodId,
                    DoctorId = _otherDoctorId,
                    DayOfWeek = testDay,
                    StartTime = "06:30",
                    EndTime = "08:00"
                };

                var updateResult = await _bll.UpdateSchedulePeriodAsync(updateDto, adminUserId, true, _otherDoctorId);
                Assert.IsTrue(updateResult.IsSuccess, $"Admin update period should succeed: {updateResult.Message}");

                // Verify updated in DB
                using (var dbCheck = new Entities())
                {
                    var period = dbCheck.DoctorSchedules.Find(newPeriodId);
                    Assert.IsNotNull(period);
                    Assert.AreEqual(TimeSpan.Parse("06:30"), period.StartTime);
                }

                // 3. Admin Deletes period for _otherDoctorId
                var deleteResult = await _bll.DeleteSchedulePeriodAsync(newPeriodId, adminUserId, true, _otherDoctorId);
                Assert.IsTrue(deleteResult.IsSuccess, $"Admin delete period should succeed: {deleteResult.Message}");

                // Verify soft deleted
                using (var dbCheck = new Entities())
                {
                    var period = dbCheck.DoctorSchedules.Find(newPeriodId);
                    Assert.IsNotNull(period);
                    Assert.IsTrue(period.IsDeleted);
                }
            }
            finally
            {
                if (newPeriodId > 0)
                {
                    using (var cleanupDb = new Entities())
                    {
                        var period = cleanupDb.DoctorSchedules.Find(newPeriodId);
                        if (period != null)
                        {
                            cleanupDb.DoctorSchedules.Remove(period);
                            cleanupDb.SaveChanges();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task Admin_SwitchesDoctors_LoadsDifferentDoctorSchedules()
        {
            var adminUserId = _testDoctorUserId;

            // 1. Admin loads Doctor A
            var resA = await _bll.GetWeeklyScheduleAsync(adminUserId, true, _testDoctorId);
            Assert.IsTrue(resA.IsSuccess);
            Assert.IsNotNull(resA.Data);
            Assert.AreEqual(_testDoctorId, resA.Data.DoctorId);

            // 2. Admin switches to Doctor B
            var resB = await _bll.GetWeeklyScheduleAsync(adminUserId, true, _otherDoctorId);
            Assert.IsTrue(resB.IsSuccess);
            Assert.IsNotNull(resB.Data);
            Assert.AreEqual(_otherDoctorId, resB.Data.DoctorId);
            Assert.AreNotEqual(resA.Data.DoctorId, resB.Data.DoctorId);
        }

        [TestMethod]
        public async Task Admin_SelectsDoctorWithNoPeriods_ReturnsEmptyScheduleWithCorrectDoctorInfo()
        {
            // Temporarily mark all active schedules of _otherDoctorId as deleted to simulate empty schedule
            var originalSchedules = _db.DoctorSchedules
                .Where(s => s.DoctorId == _otherDoctorId && !s.IsDeleted)
                .ToList();

            foreach (var s in originalSchedules)
            {
                s.IsDeleted = true;
            }
            _db.SaveChanges();

            try
            {
                var response = await _bll.GetWeeklyScheduleAsync(_testDoctorUserId, true, _otherDoctorId);
                Assert.IsTrue(response.IsSuccess);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual(_otherDoctorId, response.Data.DoctorId);
                Assert.IsNotNull(response.Data.DoctorName);
                Assert.IsNotNull(response.Data.DoctorCode);
                Assert.IsTrue(response.Data.SlotDuration > 0);
                Assert.IsNotNull(response.Data.Schedules);
                Assert.AreEqual(0, response.Data.Schedules.Count, "Doctor with no active schedules must return empty list.");
            }
            finally
            {
                using (var cleanupDb = new Entities())
                {
                    foreach (var s in originalSchedules)
                    {
                        var dbSched = cleanupDb.DoctorSchedules.Find(s.Id);
                        if (dbSched != null)
                        {
                            dbSched.IsDeleted = false;
                        }
                    }
                    cleanupDb.SaveChanges();
                }
            }
        }

        [TestMethod]
        public async Task Admin_UserWithNoDoctorProfile_CanLoadDoctorSchedule()
        {
            // Query an AspNetUser with Admin role who is NOT in Doctor table
            var adminUser = _db.AspNetUsers
                .FirstOrDefault(u => u.AspNetRoles.Any(r => r.Name == "Admin") && !_db.Doctors.Any(d => d.UserId == u.Id));

            if (adminUser != null)
            {
                var response = await _bll.GetWeeklyScheduleAsync(adminUser.Id, true, _testDoctorId);
                Assert.IsTrue(response.IsSuccess);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual(_testDoctorId, response.Data.DoctorId);
            }
        }

        [TestMethod]
        public async Task GetDoctorsAsync_ShouldReturnDoctorsListWithReceptionistNames()
        {
            var response = await _bll.GetDoctorsAsync();
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue(response.Data.Count >= 2);
            Assert.IsTrue(response.Data.Any(d => d.Id == _testDoctorId));
        }

        [TestMethod]
        public async Task UpdateDoctorAsync_WhenSchedulesIsNull_ShouldPreserveExistingSchedules()
        {
            // Ensure _testDoctorId has at least 1 schedule
            var existingSchedCount = _db.DoctorSchedules.Count(s => s.DoctorId == _testDoctorId && !s.IsDeleted);
            if (existingSchedCount == 0)
            {
                _db.DoctorSchedules.Add(new DoctorSchedule
                {
                    DoctorId = _testDoctorId,
                    DayOfWeek = 1,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    CreatedDate = DateTime.Now,
                    CreatedBy = _testDoctorUserId,
                    IsDeleted = false
                });
                _db.SaveChanges();
            }

            var doctor = _db.Doctors.Find(_testDoctorId);
            var updateDto = new DoctorUpdateDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Email = doctor.Email,
                Phone = doctor.Phone,
                SpecialityId = doctor.SpecialityId,
                ExaminationPrice = doctor.ExaminationPrice,
                SlotDuration = doctor.SlotDuration,
                IsActive = doctor.IsActive,
                Schedules = null // Explicitly null: profile edit without schedule collection
            };

            var updateResult = await _bll.UpdateDoctorAsync(updateDto, _testDoctorUserId);
            Assert.IsTrue(updateResult.IsSuccess);

            using (var checkDb = new Entities())
            {
                var countAfter = checkDb.DoctorSchedules.Count(s => s.DoctorId == _testDoctorId && !s.IsDeleted);
                Assert.IsTrue(countAfter > 0, "Existing schedules must not be deleted when dto.Schedules is null.");
            }
        }

        [TestMethod]
        public async Task Admin_DeleteSchedulePeriod_WithoutTargetDoctorId_ShouldBeRejected()
        {
            var sched = _db.DoctorSchedules.FirstOrDefault(s => s.DoctorId == _testDoctorId && !s.IsDeleted);
            Assert.IsNotNull(sched);

            // Admin calls DeletePeriod with targetDoctorId = null/0
            var result = await _bll.DeleteSchedulePeriodAsync(sched.Id, _testDoctorUserId, true, null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("schedule.PleaseSelectDoctor", result.Message);
        }

        [TestMethod]
        public async Task Admin_UpdateSchedulePeriod_WithoutTargetDoctorId_ShouldBeRejected()
        {
            var sched = _db.DoctorSchedules.FirstOrDefault(s => s.DoctorId == _testDoctorId && !s.IsDeleted);
            Assert.IsNotNull(sched);

            var dto = new DoctorScheduleDto
            {
                Id = sched.Id,
                DoctorId = 0,
                DayOfWeek = sched.DayOfWeek,
                StartTime = "09:00",
                EndTime = "17:00"
            };

            // Admin calls UpdatePeriod with targetDoctorId = null and dto.DoctorId = 0
            var result = await _bll.UpdateSchedulePeriodAsync(dto, _testDoctorUserId, true, null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("schedule.PleaseSelectDoctor", result.Message);
        }

        #endregion
    }
}
