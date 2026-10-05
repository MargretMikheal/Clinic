using Clinic.Core.BLL;
using Clinic.Core.BLL.Symptoms.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class SymptomController : Controller
    {
        private readonly SymptomsBLL _symptomsBLL;

        public SymptomController()
        {
            _symptomsBLL = new SymptomsBLL();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSymptoms()
        {
            var response = _symptomsBLL.GetSymptoms();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddSymptom(SymptomDto dto)
        {
            var response = _symptomsBLL.CreateSymptom(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult EditSymptom(SymptomDto dto)
        {
            var response = _symptomsBLL.UpdateSymptom(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult DeleteSymptom(int id)
        {
            var response = _symptomsBLL.DeleteSymptom(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}