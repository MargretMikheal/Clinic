using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Clinic.Web.Filters
{
    public class LocalizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string lang = "ar";

            HttpCookie langCookie = filterContext.HttpContext.Request.Cookies["clinic_lang"];
            if (langCookie != null && !string.IsNullOrEmpty(langCookie.Value))
            {
                lang = langCookie.Value;
            }
            else
            {
                var userLanguages = filterContext.HttpContext.Request.UserLanguages;
                if (userLanguages != null && userLanguages.Length > 0)
                {
                    string firstLang = userLanguages[0].ToLowerInvariant();
                    if (firstLang.StartsWith("ar"))
                    {
                        lang = "ar";
                    }
                }
            }

            if (lang != "ar" && lang != "en")
            {
                lang = "ar";
            }

            try
            {
                var culture = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception)
            {
                // Fallback on exception
                var culture = new CultureInfo("ar");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
