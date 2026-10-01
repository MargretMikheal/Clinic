using Clinic.Core.BLL;
using Clinic.Core.BLL.RolePermission.DTOs;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class RolePermissionsController : Controller
    {
        private readonly RolePermissionBLL _bll;

        public RolePermissionsController()
        {
            _bll = new RolePermissionBLL();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetRoles()
        {
            var response = await _bll.GetRolesWithUserCountAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> AddRole(RoleDto dto)
        {
            var response = await _bll.CreateRoleAsync(dto);
            return Json(response);
        }

        [HttpGet]
        public async Task<JsonResult> GetRolePermissions(string roleId)
        {
            var response = await _bll.GetRolePermissionsAsync(roleId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
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
