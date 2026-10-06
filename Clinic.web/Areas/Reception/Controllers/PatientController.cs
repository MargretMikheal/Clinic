using Clinic.Core.BLL;
using Clinic.Core.BLL.Patients.DTOs;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;
using Clinic.Core.Enums;
using Clinic.Core.Constants;

namespace Clinic.Web.Areas.Reception.Controllers
{
    public class PatientController : Controller
    {
        private readonly PatientsBLL _patientsBLL;

        public PatientController()
        {
            _patientsBLL = new PatientsBLL();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetPatients()
        {
            var response = await _patientsBLL.GetPatientsAsync();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetPatient(int id)
        {
            var response = await _patientsBLL.GetPatientByIdAsync(id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> AddPatient(PatientCreateDto dto)
        {
            var response = await _patientsBLL.CreatePatientAsync(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Edit)]
        public async Task<JsonResult> EditPatient(PatientUpdateDto dto)
        {
            var response = await _patientsBLL.UpdatePatientAsync(dto, User.Identity.GetUserId());
            return Json(response);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Patients, RequiredPermission = ScreenPermission.Delete)]
        public async Task<JsonResult> DeletePatient(int id)
        {
            var response = await _patientsBLL.DeletePatientAsync(id, User.Identity.GetUserId());
            return Json(response);
        }
    }
}
