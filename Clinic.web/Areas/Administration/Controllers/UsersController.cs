using Clinic.Core.BLL;
using Clinic.Core.BLL.Users.DTOs;
using System.Threading.Tasks;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Clinic.Core.Constants;
using Clinic.Web.Infrastructure.Security;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UsersBLL _usersBLL;

        public UsersController()
        {
            _usersBLL = new UsersBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Users, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Users, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetUsers()
        {
            var response = await _usersBLL.GetUsersAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRoles()
        {
            var response = _usersBLL.GetAvailableRoles();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Users, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> AddUser(UserDto dto)
        {
            var response = await _usersBLL.CreateUserAsync(dto);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Users, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> EditUser(UserDto dto)
        {
            var response = await _usersBLL.UpdateUserAsync(dto);
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Users, RequiredPermission = ScreenPermission.Delete)]
        public async Task<JsonResult> DeleteUser(string id)
        {
            var response = await _usersBLL.DeleteUserAsync(id);
            return Json(response);
        }
    }
}
