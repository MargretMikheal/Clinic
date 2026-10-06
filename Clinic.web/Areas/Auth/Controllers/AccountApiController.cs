using Clinic.Core.BLL.Auth.DTOs;
using Clinic.Core.BLL.Interfaces;
using Clinic.Core.BLL.Services;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Clinic.Core.Data;

namespace Clinic.Web.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountApiController : ApiController
    {
        private readonly IAuthenticationService _authService;

        public AccountApiController()
        {
            _authService = new AuthenticationService();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            var owinContext = HttpContext.Current.GetOwinContext();
            var response = await _authService.LoginAsync(request, owinContext);

            if (!response.IsSuccess)
            {
                string lang = GetLanguage(Request);
                if (response.Message == "Invalid username or password.")
                {
                    response.Message = lang == "ar" ? "اسم المستخدم أو كلمة المرور غير صحيحة" : "Invalid username or password";
                }
                else
                {
                    response.Message = lang == "ar" ? "خطأ في تسجيل الدخول" : response.Message;
                }
            }

            return Ok(response);
        }

        private string GetLanguage(HttpRequestMessage request)
        {
            var cookie = request.Headers.GetCookies("clinic_lang").FirstOrDefault();
            if (cookie != null && cookie["clinic_lang"] != null)
            {
                var val = cookie["clinic_lang"].Value;
                if (val == "ar" || val == "en") return val;
            }

            if (request.Headers.AcceptLanguage != null && request.Headers.AcceptLanguage.Count > 0)
            {
                var firstLang = request.Headers.AcceptLanguage.First().Value.ToLowerInvariant();
                if (firstLang.StartsWith("ar")) return "ar";
            }
            return "ar";
        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            var owinContext = HttpContext.Current.GetOwinContext();
            _authService.Logout(owinContext);
            return Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IHttpActionResult> GetCurrentUser()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return Unauthorized();
            }

            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var response = await _authService.GetCurrentUserAsync(userIdClaim.Value);
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response.Message);
        }
        [Authorize]
        [HttpGet]
        [Route("permissions")]
        public IHttpActionResult GetPermissions()
        {
            var userId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            using (var db = new Entities())
            {
                var user = db.AspNetUsers.Include("AspNetRoles").FirstOrDefault(u => u.Id == userId);
                if (user == null) return Unauthorized();

                var roleIds = user.AspNetRoles.Select(r => r.Id).ToList();
                var permissions = db.RoleScreenPermissions
                    .Where(p => roleIds.Contains(p.RoleId))
                    .GroupBy(p => p.ScreenId)
                    .Select(g => new
                    {
                        ScreenId = g.Key,
                        Show = g.Any(x => x.ScreenShow),
                        Add = g.Any(x => x.ScreenAdd),
                        Edit = g.Any(x => x.ScreenEdit),
                        Delete = g.Any(x => x.ScreenDelete),
                        Print = g.Any(x => x.ScreenPrint)
                    }).ToList();

                var permissionDict = permissions.ToDictionary(p => p.ScreenId, p => new { p.Show, p.Add, p.Edit, p.Delete, p.Print });

                return Ok(new Clinic.Core.Common.Responses.BaseResponse<object>
                {
                    IsSuccess = true,
                    Data = permissionDict
                });
            }
        }
    }
}
