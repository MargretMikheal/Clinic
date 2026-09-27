using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Mvc;

namespace Clinic.Web.Filters
{
    public class GlobalMvcExceptionFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            string errorId = Guid.NewGuid().ToString();
            
            string controllerName = (string)filterContext.RouteData.Values["controller"] ?? "Unknown";
            string actionName = (string)filterContext.RouteData.Values["action"] ?? "Unknown";

            Trace.TraceError($"[ErrorId: {errorId}] MVC Exception in {controllerName}/{actionName}: {filterContext.Exception}");

            string lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            
            filterContext.Controller.ViewData["ErrorMessage"] = lang == "ar" 
                ? "حدث خطأ غير متوقع. برجاء المحاولة مرة أخرى."
                : "An unexpected error occurred. Please try again.";
                
            filterContext.Controller.ViewData["ErrorId"] = errorId;

            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = filterContext.Controller.ViewData,
                TempData = filterContext.Controller.TempData
            };
            
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
