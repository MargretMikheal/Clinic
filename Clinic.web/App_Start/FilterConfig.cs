using System.Web.Mvc;

namespace Clinic.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new Clinic.Web.Filters.GlobalMvcExceptionFilter());
            filters.Add(new Clinic.Web.Filters.LocalizationAttribute());
        }
    }
}
