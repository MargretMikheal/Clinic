using Clinic.Core.BLL;
using Clinic.Web.Models;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Web.Mvc;

namespace Clinic.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly VisitsBLL _visitsBLL;

        public ReportController()
        {
            _visitsBLL = new VisitsBLL();
        }

        public ReportController(VisitsBLL visitsBLL)
        {
            _visitsBLL = visitsBLL;
        }

        [HttpPost]
        public ActionResult Index(ReportsParameters id, ReportsParameters model)
        {
            var reportModel = model ?? id ?? new ReportsParameters();

            if (reportModel.Parms == null)
            {
                reportModel.Parms = new System.Collections.Generic.Dictionary<string, object>();
            }

            if (Request.Form != null && Request.Form.Count > 0)
            {
                if (string.IsNullOrEmpty(reportModel.ReportName))
                {
                    reportModel.ReportName = Request.Form["ReportName"];
                }

                foreach (string key in Request.Form.AllKeys)
                {
                    if (string.IsNullOrEmpty(key) || key.Equals("ReportName", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string paramKey = key;
                    if (paramKey.StartsWith("Parms[", System.StringComparison.OrdinalIgnoreCase) && paramKey.EndsWith("]"))
                    {
                        paramKey = paramKey.Substring(6, paramKey.Length - 7);
                    }

                    if (!reportModel.Parms.ContainsKey(paramKey))
                    {
                        reportModel.Parms[paramKey] = Request.Form[key];
                    }
                }
            }

            // Normalize integer parameters
            NormalizeNumericParameters(reportModel.Parms);

            if (string.IsNullOrWhiteSpace(reportModel.ReportName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Report name is required.");
            }

            var safeReportFile = System.IO.Path.GetFileName(reportModel.ReportName);
            var reportFilePath = Server.MapPath("~/Reports/" + safeReportFile);
            if (!System.IO.File.Exists(reportFilePath))
            {
                return HttpNotFound("Report definition not found.");
            }

            if (IsPrescriptionReport(reportModel.ReportName))
            {
                int? prescriptionId = ExtractPrescriptionId(reportModel);
                if (!prescriptionId.HasValue || prescriptionId.Value <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Prescription ID is required.");
                }
                if (!IsPrescriptionAllowed(prescriptionId.Value))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Access Denied: You do not have permission to view this prescription.");
                }
            }

            if (IsPatientMedicalHistoryReport(reportModel.ReportName))
            {
                int? patientId = ExtractPatientId(reportModel);
                if (!patientId.HasValue || patientId.Value <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Patient ID is required.");
                }
            }

            return View("~/Views/Shared/ReportViewer.cshtml", reportModel);
        }

        [HttpGet]
        public ActionResult Index(string reportName, int? prescriptionId)
        {
            if (string.IsNullOrWhiteSpace(reportName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Report name is required.");
            }

            var safeReportFile = System.IO.Path.GetFileName(reportName);
            var reportFilePath = Server.MapPath("~/Reports/" + safeReportFile);
            if (!System.IO.File.Exists(reportFilePath))
            {
                return HttpNotFound("Report definition not found.");
            }

            var model = new ReportsParameters { ReportName = safeReportFile };

            if (Request.QueryString != null && Request.QueryString.Count > 0)
            {
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (string.IsNullOrEmpty(key) || key.Equals("reportName", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    string paramKey = key;
                    if (paramKey.StartsWith("Parms[", System.StringComparison.OrdinalIgnoreCase) && paramKey.EndsWith("]"))
                    {
                        paramKey = paramKey.Substring(6, paramKey.Length - 7);
                    }

                    model.Parms[paramKey] = Request.QueryString[key];
                }
            }

            if (prescriptionId.HasValue)
            {
                model.Parms["PrescriptionId"] = prescriptionId.Value;
            }

            // Normalize integer parameters
            NormalizeNumericParameters(model.Parms);

            if (IsPrescriptionReport(reportName))
            {
                int? rxId = ExtractPrescriptionId(model);
                if (!rxId.HasValue || rxId.Value <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Prescription ID is required.");
                }
                if (!IsPrescriptionAllowed(rxId.Value))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Access Denied: You do not have permission to view this prescription.");
                }
            }

            if (IsPatientMedicalHistoryReport(reportName))
            {
                int? patientId = ExtractPatientId(model);
                if (!patientId.HasValue || patientId.Value <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Patient ID is required.");
                }
            }

            return View("~/Views/Shared/ReportViewer.cshtml", model);
        }

        internal static void NormalizeNumericParameters(System.Collections.Generic.Dictionary<string, object> parms)
        {
            if (parms == null) return;

            var keys = new System.Collections.Generic.List<string>(parms.Keys);
            foreach (var k in keys)
            {
                var val = parms[k];
                if (val == null) continue;

                // Handle array / collection values (e.g. from MVC DefaultModelBinder RawValue)
                var enumerable = val as System.Collections.IEnumerable;
                if (enumerable != null && !(val is string))
                {
                    object first = null;
                    int count = 0;
                    foreach (var item in enumerable)
                    {
                        if (count == 0) first = item;
                        count++;
                    }

                    if (count == 0)
                    {
                        parms[k] = null;
                        continue;
                    }
                    if (count == 1)
                    {
                        val = first;
                        parms[k] = val;
                    }
                }

                if (val == null) continue;

                var strVal = val as string;
                if (strVal != null)
                {
                    strVal = strVal.Trim();
                    int parsedInt;
                    if (int.TryParse(strVal, out parsedInt))
                    {
                        parms[k] = parsedInt;
                    }
                    else if (strVal.Length == 0)
                    {
                        parms[k] = null;
                    }
                    else
                    {
                        parms[k] = strVal;
                    }
                }
            }
        }

        private static bool IsPrescriptionReport(string reportName)
        {
            return !string.IsNullOrEmpty(reportName) &&
                   reportName.IndexOf("PrescriptionReport", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsPatientMedicalHistoryReport(string reportName)
        {
            return !string.IsNullOrEmpty(reportName) &&
                   reportName.IndexOf("PatientMedicalHistory", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static int? ExtractId(ReportsParameters model, string parameterName)
        {
            if (model?.Parms == null) return null;

            foreach (var kvp in model.Parms)
            {
                if (string.Equals(kvp.Key, parameterName, System.StringComparison.OrdinalIgnoreCase))
                {
                    var val = kvp.Value;
                    if (val == null) return null;

                    if (val is int) return (int)val;
                    if (val is long) return (int)(long)val;

                    var enumerable = val as System.Collections.IEnumerable;
                    if (enumerable != null && !(val is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null)
                            {
                                int itemParsed;
                                if (int.TryParse(item.ToString(), out itemParsed))
                                    return itemParsed;
                            }
                        }
                        return null;
                    }

                    int parsed;
                    if (int.TryParse(val.ToString(), out parsed))
                    {
                        return parsed;
                    }
                }
            }
            return null;
        }

        private static int? ExtractPrescriptionId(ReportsParameters model)
        {
            return ExtractId(model, "PrescriptionId");
        }

        private static int? ExtractPatientId(ReportsParameters model)
        {
            return ExtractId(model, "PatientId");
        }

        private bool IsPrescriptionAllowed(int prescriptionId)
        {
            var currentUserId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            return _visitsBLL.ValidatePrescriptionAccess(currentUserId, isAdmin, prescriptionId);
        }
    }
}
