using Clinic.Core.BLL;
using Clinic.Core.BLL.Diagnoses.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Web.Infrastructure.Security;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class DiagnosisController : Controller
    {
        private readonly DiagnosesBLL _diagnosesBLL;

        public DiagnosisController()
        {
            _diagnosesBLL = new DiagnosesBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Diagnoses, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Diagnoses, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetDiagnoses()
        {
            var response = _diagnosesBLL.GetDiagnoses();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Diagnoses, RequiredPermission = ScreenPermission.Add)]
        public JsonResult AddDiagnosis(DiagnosisDto dto)
        {
            var response = _diagnosesBLL.CreateDiagnosis(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Diagnoses, RequiredPermission = ScreenPermission.Edit)]
        public JsonResult EditDiagnosis(DiagnosisDto dto)
        {
            var response = _diagnosesBLL.UpdateDiagnosis(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Diagnoses, RequiredPermission = ScreenPermission.Delete)]
        public JsonResult DeleteDiagnosis(int id)
        {
            var response = _diagnosesBLL.DeleteDiagnosis(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
