using Clinic.Core.BLL.Interfaces;
using Clinic.Core.BLL.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Clinic.Web.Controllers
{
    [Authorize]
    [RoutePrefix("api/dashboard")]
    public class DashboardApiController : ApiController
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IReceptionistDashboardService _receptionistDashboardService;
        private readonly IDoctorDashboardService _doctorDashboardService;

        public DashboardApiController()
        {
            _adminDashboardService = new AdminDashboardService();
            _receptionistDashboardService = new ReceptionistDashboardService();
            _doctorDashboardService = new DoctorDashboardService();
        }

        // =========================================================
        // Admin
        // =========================================================

        [HttpGet]
        [Route("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> GetAdminDashboard()
        {
            var response = await _adminDashboardService.GetDashboardAsync();

            return Ok(response);
        }

        // =========================================================
        // Receptionist
        // =========================================================

        [HttpGet]
        [Route("receptionist")]
        [Authorize(Roles = "Receptionist")]
        public async Task<IHttpActionResult> GetReceptionistDashboard()
        {
            var userId = GetCurrentUserId();

            var response = await _receptionistDashboardService.GetDashboardAsync(userId);

            return Ok(response);
        }

        // =========================================================
        // Doctor
        // =========================================================

        [HttpGet]
        [Route("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IHttpActionResult> GetDoctorDashboard()
        {
            var userId = GetCurrentUserId();

            var response = await _doctorDashboardService.GetDashboardAsync(userId);

            return Ok(response);
        }

        // =========================================================
        // Helper
        // =========================================================

        private string GetCurrentUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity == null)
            {
                return null;
            }

            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return userIdClaim != null ? userIdClaim.Value : null;
        }
    }
}