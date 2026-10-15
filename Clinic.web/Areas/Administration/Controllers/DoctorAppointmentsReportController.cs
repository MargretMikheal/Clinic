using Clinic.Core.BLL;
using Clinic.Core.BLL.Doctors.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Clinic.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class DoctorAppointmentsReportController : Controller
    {
        private readonly DoctorsBLL _doctorsBLL;

        public DoctorAppointmentsReportController()
        {
            _doctorsBLL = new DoctorsBLL();
        }

        public DoctorAppointmentsReportController(DoctorsBLL doctorsBLL)
        {
            _doctorsBLL = doctorsBLL;
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.DoctorAppointmentsReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View("~/Areas/Administration/Views/DoctorAppointments/Index.cshtml");
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.DoctorAppointmentsReport, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetFilterData()
        {
            try
            {
                var doctorsResponse = await _doctorsBLL.GetActiveDoctorsForScheduleAsync();
                var doctors = doctorsResponse != null && doctorsResponse.IsSuccess && doctorsResponse.Data != null
                    ? doctorsResponse.Data
                    : new List<DoctorListDto>();

                var statuses = new List<string>
                {
                    AppointmentStatus.Booked.ToString(),
                    AppointmentStatus.Confirmed.ToString(),
                    AppointmentStatus.Completed.ToString(),
                    AppointmentStatus.Cancelled.ToString(),
                    AppointmentStatus.NoShow.ToString()
                };

                return Json(BaseResponse<object>.Success(new
                {
                    Doctors = doctors,
                    Statuses = statuses
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(BaseResponse<object>.Fail(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.DoctorAppointmentsReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult ViewReport(int? doctorId, string fromDate, string toDate, string status)
        {
            var model = new ReportsParameters
            {
                ReportName = "DoctorAppointmentsReport.trdx",
                Parms = new Dictionary<string, object>
                {
                    { "DoctorId", doctorId.GetValueOrDefault(0) },
                    { "FromDate", string.IsNullOrWhiteSpace(fromDate) ? null : fromDate.Trim() },
                    { "ToDate", string.IsNullOrWhiteSpace(toDate) ? null : toDate.Trim() },
                    { "Status", string.IsNullOrWhiteSpace(status) ? null : status.Trim() }
                }
            };

            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }
    }
}
