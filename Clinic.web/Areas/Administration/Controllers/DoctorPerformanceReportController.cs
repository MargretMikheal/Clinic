using Clinic.Core.Constants;
using Clinic.Core.Enums;
using Clinic.Web.Infrastructure.Security;
using Clinic.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Clinic.Web.Areas.Administration.Controllers
{
    [Authorize]
    public class DoctorPerformanceReportController : Controller
    {
        [HttpGet]
        [ScreenAuthorize(ScreenId = ScreenKeys.DoctorPerformanceReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult Index()
        {
            return View("~/Areas/Administration/Views/DoctorPerformance/Index.cshtml");
        }

        [HttpPost]
        [ScreenAuthorize(ScreenId = ScreenKeys.DoctorPerformanceReport, RequiredPermission = ScreenPermission.Show)]
        public ActionResult ViewReport(string fromDate, string toDate)
        {
            var model = new ReportsParameters
            {
                ReportName = "DoctorPerformanceReport.trdx",
                Parms = new Dictionary<string, object>
                {
                    { "FromDate", string.IsNullOrWhiteSpace(fromDate) ? null : fromDate.Trim() },
                    { "ToDate", string.IsNullOrWhiteSpace(toDate) ? null : toDate.Trim() }
                }
            };

            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }
    }
}
