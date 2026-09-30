using Clinic.Core.BLL;
using Clinic.Core.BLL.Doctors.DTOs;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize(Roles = nameof(Clinic.Core.Enums.UserRole.Admin))]
    public class DoctorsController : Controller
    {
        private readonly DoctorsBLL _doctorsBLL;

        public DoctorsController()
        {
            _doctorsBLL = new DoctorsBLL();
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctors()
        {
            var response = await _doctorsBLL.GetDoctorsAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctor(int id)
        {
            var response = await _doctorsBLL.GetDoctorByIdAsync(id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctorSpecialities()
        {
            var response = await _doctorsBLL.GetDoctorSpecialitiesAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctorReceptionists()
        {
            var response = await _doctorsBLL.GetDoctorReceptionistsAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetNextDoctorCode()
        {
            var response = await _doctorsBLL.GetNextDoctorCodeAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> AddDoctor(DoctorCreateDto dto)
        {
            var response = await _doctorsBLL.CreateDoctorAsync(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> EditDoctor(DoctorUpdateDto dto)
        {
            var response = await _doctorsBLL.UpdateDoctorAsync(dto, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
