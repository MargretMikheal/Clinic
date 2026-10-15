using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using Clinic.Core.BLL;
using Microsoft.AspNet.Identity;
using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.WebApi;

namespace Clinic.Web.Controllers
{
    [System.Web.Http.Authorize]
    public class ReportsController : ReportsControllerBase
    {
        static readonly ReportServiceConfiguration configurationInstance;
        private readonly VisitsBLL _visitsBLL;

        static ReportsController()
        {
            var appPath = HostingEnvironment.MapPath("~/") ?? (HttpContext.Current != null ? HttpContext.Current.Server.MapPath("~/") : HttpRuntime.AppDomainAppPath);
            var reportsPath = Path.Combine(appPath, "Reports");
            var storagePath = Path.Combine(appPath, "reportstemp");

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var resolver = new ReportFileResolver(reportsPath)
                .AddFallbackResolver(new ReportTypeResolver());

            configurationInstance = new ReportServiceConfiguration
            {
                HostAppId = "Html5App",
                Storage = new FileStorage(storagePath),
                ReportResolver = resolver,
            };
        }

        public ReportsController()
        {
            this.ReportServiceConfiguration = configurationInstance;
            this._visitsBLL = new VisitsBLL();
        }

        public ReportsController(VisitsBLL visitsBLL)
        {
            this.ReportServiceConfiguration = configurationInstance;
            this._visitsBLL = visitsBLL;
        }

        public override HttpResponseMessage CreateInstance(string clientID, ClientReportSource reportSource)
        {
            NormalizeClientParameters(reportSource);
            var authCheck = ValidatePrescriptionScope(reportSource);
            if (authCheck != null)
            {
                return authCheck;
            }
            return base.CreateInstance(clientID, reportSource);
        }

        public override HttpResponseMessage GetParameters(string clientID, ClientReportSource reportSource)
        {
            NormalizeClientParameters(reportSource);
            var authCheck = ValidatePrescriptionScope(reportSource);
            if (authCheck != null)
            {
                return authCheck;
            }
            return base.GetParameters(clientID, reportSource);
        }

        private static void NormalizeClientParameters(ClientReportSource reportSource)
        {
            if (reportSource?.ParameterValues == null) return;

            var keys = new System.Collections.Generic.List<string>(reportSource.ParameterValues.Keys);
            foreach (var k in keys)
            {
                var val = reportSource.ParameterValues[k];
                if (val == null) continue;

                var jArray = val as Newtonsoft.Json.Linq.JArray;
                if (jArray != null)
                {
                    if (jArray.Count == 0)
                    {
                        reportSource.ParameterValues[k] = null;
                        continue;
                    }
                    if (jArray.Count == 1)
                    {
                        var first = jArray[0] as Newtonsoft.Json.Linq.JValue;
                        if (first != null)
                        {
                            val = first.Value;
                            reportSource.ParameterValues[k] = val;
                        }
                    }
                }

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
                        reportSource.ParameterValues[k] = null;
                        continue;
                    }
                    if (count == 1)
                    {
                        val = first;
                        reportSource.ParameterValues[k] = val;
                    }
                }

                if (val != null)
                {
                    int parsedInt;
                    if (int.TryParse(val.ToString(), out parsedInt))
                    {
                        reportSource.ParameterValues[k] = parsedInt;
                    }
                    else if (val is string str && str.Trim().Length == 0)
                    {
                        reportSource.ParameterValues[k] = null;
                    }
                }
            }
        }

        private HttpResponseMessage ValidatePrescriptionScope(ClientReportSource reportSource)
        {
            if (reportSource != null && !string.IsNullOrEmpty(reportSource.Report) &&
                reportSource.Report.IndexOf("PrescriptionReport", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var principal = User ?? (HttpContext.Current != null ? HttpContext.Current.User : null);
                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "Authentication required.");
                }

                int? prescriptionId = null;
                if (reportSource.ParameterValues != null && reportSource.ParameterValues.ContainsKey("PrescriptionId"))
                {
                    var val = reportSource.ParameterValues["PrescriptionId"];
                    if (val != null)
                    {
                        int parsed;
                        if (int.TryParse(val.ToString(), out parsed))
                        {
                            prescriptionId = parsed;
                        }
                    }
                }

                if (prescriptionId.HasValue)
                {
                    var userId = principal.Identity.GetUserId();
                    var isAdmin = principal.IsInRole("Admin");
                    if (!_visitsBLL.ValidatePrescriptionAccess(userId, isAdmin, prescriptionId.Value))
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden, "Access Denied: You do not have permission to view this prescription.");
                    }
                }
            }
            return null;
        }
    }
}
