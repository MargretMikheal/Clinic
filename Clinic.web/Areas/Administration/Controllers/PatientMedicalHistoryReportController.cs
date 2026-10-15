using Clinic.Core.BLL;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Clinic.Web.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class PatientMedicalHistoryReportController : Controller
    {
        private readonly PatientsBLL _patientsBLL;

        public PatientMedicalHistoryReportController()
        {
            _patientsBLL = new PatientsBLL();
        }

        public PatientMedicalHistoryReportController(PatientsBLL patientsBLL)
        {
            _patientsBLL = patientsBLL;
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientMedicalHistoryReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View("~/Areas/Administration/Views/PatientMedicalHistory/Index.cshtml");
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientMedicalHistoryReport, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> SearchPatients(string term = null)
        {
            var response = await _patientsBLL.GetPatientsForHistoryAsync(null, true, term);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.PatientMedicalHistoryReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult ViewReport(int? patientId)
        {
            if (!patientId.HasValue || patientId.Value <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Patient ID is required.");
            }

            var model = new ReportsParameters
            {
                ReportName = "PatientMedicalHistoryReport.trdx",
                Parms = new Dictionary<string, object>
                {
                    { "PatientId", patientId.Value }
                }
            };

            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }
    }
}
