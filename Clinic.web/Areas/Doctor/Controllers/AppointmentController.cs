using Clinic.Core.BLL;
using Clinic.Core.BLL.Appointments.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Doctor.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppointmentBLL _appointmentBLL;

        public AppointmentController() : this(new AppointmentBLL())
        {
        }

        public AppointmentController(AppointmentBLL appointmentBLL)
        {
            _appointmentBLL = appointmentBLL;
        }

        private string CurrentUserId => User?.Identity?.GetUserId();

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MyAppointments, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MyAppointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetMyAppointments(DateTime? fromDate, DateTime? toDate, string status, string searchText)
        {
            var response = await _appointmentBLL.GetAppointmentsAsync(
                CurrentUserId,
                isAdmin: false,
                isReceptionist: false,
                isDoctor: true,
                fromDate: fromDate,
                toDate: toDate,
                doctorId: null,
                status: status,
                searchText: searchText);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MyAppointments, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetAppointments(DateTime? fromDate, DateTime? toDate, string status, string searchText)
        {
            return await GetMyAppointments(fromDate, toDate, status, searchText);
        }
    }
}
