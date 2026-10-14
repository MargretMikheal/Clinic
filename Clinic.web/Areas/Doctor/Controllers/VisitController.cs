using Clinic.Core.BLL;
using Clinic.Core.BLL.Visits;
using Clinic.Core.Common.Responses;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Doctor.Controllers
{
    [Authorize]
    public class VisitController : Controller
    {
        private readonly VisitsBLL _visitsBLL;
        private readonly SymptomsBLL _symptomsBLL;
        private readonly DiagnosesBLL _diagnosesBLL;

        public VisitController() : this(new VisitsBLL(), new SymptomsBLL(), new DiagnosesBLL())
        {
        }

        public VisitController(VisitsBLL visitsBLL, SymptomsBLL symptomsBLL, DiagnosesBLL diagnosesBLL)
        {
            _visitsBLL = visitsBLL;
            _symptomsBLL = symptomsBLL;
            _diagnosesBLL = diagnosesBLL;
        }

        private string CurrentUserId => User?.Identity?.GetUserId();

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> GetTodayAppointments()
        {
            var response = await _visitsBLL.GetTodayEligibleAppointmentsAsync(CurrentUserId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetMasterData()
        {
            var symptomsRes = _symptomsBLL.GetActiveSymptoms();
            if (!symptomsRes.IsSuccess)
            {
                return Json(BaseResponse<object>.Fail(symptomsRes.Message), JsonRequestBehavior.AllowGet);
            }

            var diagnosesRes = _diagnosesBLL.GetActiveDiagnoses();
            if (!diagnosesRes.IsSuccess)
            {
                return Json(BaseResponse<object>.Fail(diagnosesRes.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(BaseResponse<object>.Success(new
            {
                Symptoms = symptomsRes.Data,
                Diagnoses = diagnosesRes.Data
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetSymptoms()
        {
            var response = _symptomsBLL.GetActiveSymptoms();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Show)]
        public JsonResult GetDiagnoses()
        {
            var response = _diagnosesBLL.GetActiveDiagnoses();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.Visit, RequiredPermission = ScreenPermission.Add)]
        public async Task<JsonResult> SaveVisit(Clinic.Core.BLL.Visits.DTOs.VisitFormDto dto)
        {
            var response = await _visitsBLL.SaveVisitAsync(dto, CurrentUserId);
            return Json(response);
        }
    }
}
