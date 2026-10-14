namespace Clinic.Web.Controllers
{
    using System.IO;
    using System.Web;
    using System.Web.Hosting;
    using Telerik.Reporting.Cache.File;
    using Telerik.Reporting.Services;
    using Telerik.Reporting.Services.WebApi;

    public class ReportsController : ReportsControllerBase
    {
        static readonly ReportServiceConfiguration configurationInstance;

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
        }
    }
}
