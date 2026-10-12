using Clinic.Core.BLL;
using Clinic.Core.BLL.Patients.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Doctor.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly PatientsBLL _patientsBLL;

        public PatientController() : this(new PatientsBLL())
        {
        }

        public PatientController(PatientsBLL patientsBLL)
        {
            _patientsBLL = patientsBLL;
        }

        private bool IsAdmin => User.IsInRole("Admin");
        private bool IsDoctor => User.IsInRole("Doctor");
        private string CurrentUserId => User?.Identity?.GetUserId();

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientHistory, RequiredPermission = ScreenPermission.Show)]
        public ActionResult History()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientHistory, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetCurrentRole()
        {
            return Json(BaseResponse<object>.Success(new
            {
                IsAdmin = IsAdmin,
                IsDoctor = IsDoctor
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientHistory, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetPatients()
        {
            if (!IsAdmin && !IsDoctor)
            {
                return Json(BaseResponse<List<PatientHistoryListItemDto>>.Fail("Unauthorized access."), JsonRequestBehavior.AllowGet);
            }

            var response = await _patientsBLL.GetPatientsForHistoryAsync(CurrentUserId, IsAdmin);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientHistory, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetPatientHistory(int? id, int? patientId)
        {
            int targetId = id ?? patientId ?? 0;

            if (!IsAdmin && !IsDoctor)
            {
                return Json(BaseResponse<PatientHistoryDetailsDto>.Fail("Unauthorized access."), JsonRequestBehavior.AllowGet);
            }

            if (targetId <= 0)
            {
                return Json(BaseResponse<PatientHistoryDetailsDto>.Fail("Invalid patient ID."), JsonRequestBehavior.AllowGet);
            }

            var response = await _patientsBLL.GetPatientHistoryDetailsAsync(targetId, CurrentUserId, IsAdmin);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
