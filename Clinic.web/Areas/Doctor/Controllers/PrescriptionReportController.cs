using Clinic.Core.BLL;
using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Clinic.Web.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Doctor.Controllers
{
    [Authorize]
    public class PrescriptionReportController : Controller
    {
        private readonly VisitsBLL _visitsBLL;

        public PrescriptionReportController()
        {
            _visitsBLL = new VisitsBLL();
        }

        public PrescriptionReportController(VisitsBLL visitsBLL)
        {
            _visitsBLL = visitsBLL;
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PrescriptionReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.PrescriptionReport, RequiredPermission = ScreenPermission.Show)]
        public async Task<JsonResult> SearchPrescriptions(string term = null, int? prescriptionId = null)
        {
            var currentUserId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var response = await _visitsBLL.GetPrescriptionsForReportAsync(currentUserId, isAdmin, term, prescriptionId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.PrescriptionReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult ViewReport(int? prescriptionId)
        {
            if (!prescriptionId.HasValue || prescriptionId.Value <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Prescription ID is required.");
            }

            var currentUserId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            if (!_visitsBLL.ValidatePrescriptionAccess(currentUserId, isAdmin, prescriptionId.Value))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Access Denied: You do not have permission to view this prescription.");
            }

            var model = new ReportsParameters
            {
                ReportName = "PrescriptionReport.trdx",
                Parms = new Dictionary<string, object>
                {
                    { "PrescriptionId", prescriptionId.Value }
                }
            };

            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }
    }
}
