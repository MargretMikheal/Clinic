using Clinic.Core.BLL;
using Clinic.Core.BLL.Doctors.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Doctor.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly DoctorsBLL _doctorsBLL;

        public ScheduleController()
        {
            _doctorsBLL = new DoctorsBLL();
        }

        public ScheduleController(DoctorsBLL doctorsBLL)
        {
            _doctorsBLL = doctorsBLL;
        }

        private bool IsAdmin => User.IsInRole("Admin");
        private bool IsDoctor => User.IsInRole("Doctor");
        private string CurrentUserId => User.Identity.GetUserId();

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetMySchedule(int? doctorId)
        {
            var response = await _doctorsBLL.GetWeeklyScheduleAsync(CurrentUserId, IsAdmin, IsAdmin ? doctorId : null);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetActiveDoctors()
        {
            if (!IsAdmin)
            {
                return Json(BaseResponse<System.Collections.Generic.List<DoctorListDto>>.Fail("schedule.Unauthorized"), JsonRequestBehavior.AllowGet);
            }
            var response = await _doctorsBLL.GetActiveDoctorsForScheduleAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetCurrentRole()
        {
            return Json(BaseResponse<object>.Success(new
            {
                IsAdmin = IsAdmin,
                IsDoctor = IsDoctor
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> AddPeriod(DoctorScheduleDto dto, int? doctorId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(BaseResponse<DoctorScheduleDto>.Fail("schedule.InvalidData", errors));
            }

            var targetDocId = IsAdmin ? (doctorId ?? dto.DoctorId) : (int?)null;
            var response = await _doctorsBLL.AddSchedulePeriodAsync(dto, CurrentUserId, IsAdmin, targetDocId);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> UpdatePeriod(DoctorScheduleDto dto, int? doctorId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(BaseResponse<bool>.Fail("schedule.InvalidData", errors));
            }

            var targetDocId = IsAdmin ? (doctorId ?? dto.DoctorId) : (int?)null;
            var response = await _doctorsBLL.UpdateSchedulePeriodAsync(dto, CurrentUserId, IsAdmin, targetDocId);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.MySchedule, RequiredPermission = ScreenPermission.Delete)]
        public async Task<JsonResult> DeletePeriod(int id, int? doctorId)
        {
            var targetDocId = IsAdmin ? doctorId : (int?)null;
            var response = await _doctorsBLL.DeleteSchedulePeriodAsync(id, CurrentUserId, IsAdmin, targetDocId);
            return Json(response);
        }
    }
}
