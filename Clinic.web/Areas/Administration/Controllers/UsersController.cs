using Clinic.Core.BLL;
using Clinic.Core.BLL.Users.DTOs;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class UsersController : Controller
    {
        private readonly UsersBLL _usersBLL;

        public UsersController()
        {
            _usersBLL = new UsersBLL();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
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
        public async Task<JsonResult> AddUser(UserDto dto)
        {
            var response = await _usersBLL.CreateUserAsync(dto);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> EditUser(UserDto dto)
        {
            var response = await _usersBLL.UpdateUserAsync(dto);
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser(string id)
        {
            var response = await _usersBLL.DeleteUserAsync(id);
            return Json(response);
        }
    }
}
