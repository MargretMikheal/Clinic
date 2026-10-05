using Clinic.Core.BLL;
using Clinic.Core.BLL.Specialities.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class SpecialityController : Controller
    {
        private readonly SpecialitiesBLL _specialitiesBLL;

        public SpecialityController()
        {
            _specialitiesBLL = new SpecialitiesBLL();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSpecialities()
        {
            var response = _specialitiesBLL.GetSpecialities();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddSpeciality(SpecialityDto dto)
        {
            var response = _specialitiesBLL.CreateSpeciality(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult EditSpeciality(SpecialityDto dto)
        {
            var response = _specialitiesBLL.UpdateSpeciality(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public JsonResult DeleteSpeciality(int id)
        {
            var response = _specialitiesBLL.DeleteSpeciality(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
