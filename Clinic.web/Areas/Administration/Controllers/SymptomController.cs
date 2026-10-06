using Clinic.Core.BLL;
using Clinic.Core.BLL.Symptoms.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Web.Infrastructure.Security;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class SymptomController : Controller
    {
        private readonly SymptomsBLL _symptomsBLL;

        public SymptomController()
        {
            _symptomsBLL = new SymptomsBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Symptoms, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Symptoms, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetSymptoms()
        {
            var response = _symptomsBLL.GetSymptoms();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Symptoms, RequiredPermission = ScreenPermission.Add)]
        public JsonResult AddSymptom(SymptomDto dto)
        {
            var response = _symptomsBLL.CreateSymptom(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Symptoms, RequiredPermission = ScreenPermission.Edit)]
        public JsonResult EditSymptom(SymptomDto dto)
        {
            var response = _symptomsBLL.UpdateSymptom(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Symptoms, RequiredPermission = ScreenPermission.Delete)]
        public JsonResult DeleteSymptom(int id)
        {
            var response = _symptomsBLL.DeleteSymptom(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}