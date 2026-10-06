using Clinic.Core.BLL;
using Clinic.Core.BLL.RolePermission.DTOs;
using System.Threading.Tasks;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Web.Infrastructure.Security;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class RolePermissionsController : Controller
    {
        private readonly RolePermissionBLL _bll;

        public RolePermissionsController()
        {
            _bll = new RolePermissionBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.RolePermissions, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.RolePermissions, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetRoles()
        {
            var response = await _bll.GetRolesWithUserCountAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.RolePermissions, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> AddRole(RoleDto dto)
        {
            var response = await _bll.CreateRoleAsync(dto);
            return Json(response);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.RolePermissions, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetRolePermissions(string roleId)
        {
            var response = await _bll.GetRolePermissionsAsync(roleId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.RolePermissions, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> SaveRolePermissions(SaveRoleScreenPermissionDto dto)
        {
            var response = await _bll.SaveRolePermissionsAsync(dto);
            if (response.IsSuccess)
            {
                Clinic.Web.Infrastructure.SiteMap.ClinicVisibilityProvider.ClearCache();
            }
            return Json(response);
        }
    }
}
