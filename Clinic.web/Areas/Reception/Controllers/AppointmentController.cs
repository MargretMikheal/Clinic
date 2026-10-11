using Clinic.Core.BLL;
using Clinic.Core.BLL.Appointments.DTOs;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Core.Common.Responses;

namespace Clinic.Web.Areas.Reception.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppointmentBLL _appointmentBLL;

        public AppointmentController()
        {
            _appointmentBLL = new AppointmentBLL();
        }

        private bool IsAdmin => User.IsInRole("Admin");
        private bool IsReceptionist => User.IsInRole("Receptionist");
        private bool IsDoctor => User.IsInRole("Doctor");
        private string CurrentUserId => User.Identity.GetUserId();

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetAppointmentDoctors()
        {
            var response = await _appointmentBLL.GetAppointmentDoctorsAsync(CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetAppointments(DateTime? fromDate, DateTime? toDate, int? doctorId, string status, string searchText)
        {
            var response = await _appointmentBLL.GetAppointmentsAsync(CurrentUserId, IsAdmin, IsReceptionist, IsDoctor, fromDate, toDate, doctorId, status, searchText);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetAppointment(int id)
        {
            var response = await _appointmentBLL.GetAppointmentByIdAsync(id, CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var response = await _appointmentBLL.GetAvailableSlotsAsync(doctorId, date);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> AddAppointment(CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(BaseResponse<bool>.Fail("Invalid data provided.", errors));
            }

            var response = await _appointmentBLL.CreateAppointmentAsync(dto, CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> EditAppointment(UpdateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(BaseResponse<bool>.Fail("Invalid data provided.", errors));
            }

            var response = await _appointmentBLL.UpdateAppointmentAsync(dto, CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> ChangeStatus(ChangeAppointmentStatusDto dto)
        {
            if (!ModelState.IsValid)
                return Json(BaseResponse<bool>.Fail("Invalid request"));

            var response = await _appointmentBLL.ChangeAppointmentStatusAsync(dto, CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Appointments, RequiredPermission = ScreenPermission.Delete)]
        public async Task<JsonResult> DeleteAppointment(int id)
        {
            var response = await _appointmentBLL.DeleteAppointmentAsync(id, CurrentUserId, IsAdmin, IsReceptionist, IsDoctor);
            return Json(response);
        }
    }
}
