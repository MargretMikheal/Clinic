using Clinic.Web.Models;
using System.Web.Mvc;

namespace Clinic.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        [HttpPost]
        public ActionResult Index(ReportsParameters id, ReportsParameters model)
        {
            var reportModel = id ?? model ?? new ReportsParameters();
            return PartialView("~/Views/Shared/ReportViewer.cshtml", reportModel);
        }

        [HttpGet]
        public ActionResult Index(string reportName, int? prescriptionId)
        {
            var model = new ReportsParameters { ReportName = reportName };
            if (prescriptionId.HasValue)
            {
                model.Parms["PrescriptionId"] = prescriptionId.Value;
            }
            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }
    }
}
