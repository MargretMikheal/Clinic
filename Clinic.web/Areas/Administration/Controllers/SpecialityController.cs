using Clinic.Core.BLL;
using Clinic.Core.BLL.Specialities.DTOs;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Web.Infrastructure.Security;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class SpecialityController : Controller
    {
        private readonly SpecialitiesBLL _specialitiesBLL;

        public SpecialityController()
        {
            _specialitiesBLL = new SpecialitiesBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Specialities, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Specialities, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetSpecialities()
        {
            var response = _specialitiesBLL.GetSpecialities();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Specialities, RequiredPermission = ScreenPermission.Add)]
        public JsonResult AddSpeciality(SpecialityDto dto)
        {
            var response = _specialitiesBLL.CreateSpeciality(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Specialities, RequiredPermission = ScreenPermission.Edit)]
        public JsonResult EditSpeciality(SpecialityDto dto)
        {
            var response = _specialitiesBLL.UpdateSpeciality(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Specialities, RequiredPermission = ScreenPermission.Delete)]
        public JsonResult DeleteSpeciality(int id)
        {
            var response = _specialitiesBLL.DeleteSpeciality(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
