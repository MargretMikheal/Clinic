using Clinic.Core.BLL;
using Clinic.Core.BLL.Diagnoses.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class DiagnosisController : Controller
    {
        private readonly DiagnosesBLL _diagnosesBLL;

        public DiagnosisController()
        {
            _diagnosesBLL = new DiagnosesBLL();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetDiagnoses()
        {
            var response = _diagnosesBLL.GetDiagnoses();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddDiagnosis(DiagnosisDto dto)
        {
            var response = _diagnosesBLL.CreateDiagnosis(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult EditDiagnosis(DiagnosisDto dto)
        {
            var response = _diagnosesBLL.UpdateDiagnosis(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult DeleteDiagnosis(int id)
        {
            var response = _diagnosesBLL.DeleteDiagnosis(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
