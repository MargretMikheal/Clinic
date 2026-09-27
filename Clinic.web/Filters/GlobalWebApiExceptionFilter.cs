using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Clinic.Core.Common.Responses;

namespace Clinic.Web.Filters
{
    public class GlobalWebApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            string errorId = Guid.NewGuid().ToString();
            
            string actionName = context.ActionContext?.ActionDescriptor?.ActionName ?? "UnknownAction";
            Trace.TraceError($"[ErrorId: {errorId}] Web API Exception in {actionName}: {context.Exception}");

            string lang = GetLanguage(context.Request);
            string message = lang == "ar" 
                ? "حدث خطأ غير متوقع. برجاء المحاولة مرة أخرى."
                : "An unexpected error occurred. Please try again.";

            var response = new BaseResponse
            {
                IsSuccess = false,
                Message = message,
                ErrorId = errorId
            };

            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        private string GetLanguage(HttpRequestMessage request)
        {
            var cookie = request.Headers.GetCookies("clinic_lang").FirstOrDefault();
            if (cookie != null && cookie["clinic_lang"] != null)
            {
                var val = cookie["clinic_lang"].Value;
                if (val == "ar" || val == "en") return val;
            }
            
            if (request.Headers.AcceptLanguage != null && request.Headers.AcceptLanguage.Count > 0)
            {
                var firstLang = request.Headers.AcceptLanguage.First().Value.ToLowerInvariant();
                if (firstLang.StartsWith("ar")) return "ar";
            }
            return "ar"; 
        }
    }
}
