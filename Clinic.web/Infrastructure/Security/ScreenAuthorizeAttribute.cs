using Clinic.Core.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Clinic.Core.Enums;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace Clinic.Web.Infrastructure.Security
{
    public class ScreenAuthorizeAttribute : AuthorizeAttribute
    {
        public int ScreenId { get; set; }
        public ScreenPermission RequiredPermission { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var userId = httpContext.User.Identity.GetUserId();

            using (var db = new Entities())
            {
                var user = db.AspNetUsers.Include("AspNetRoles").FirstOrDefault(u => u.Id == userId);
                if (user == null) return false;

                var roleIds = user.AspNetRoles.Select(r => r.Id).ToList();

                var permissions = db.RoleScreenPermissions
                    .Where(p => p.ScreenId == ScreenId && roleIds.Contains(p.RoleId))
                    .ToList();

                if (!permissions.Any()) return false;

                switch (RequiredPermission)
                {
                    case ScreenPermission.Show:
                        return permissions.Any(p => p.ScreenShow);
                    case ScreenPermission.Add:
                        return permissions.Any(p => p.ScreenAdd);
                    case ScreenPermission.Edit:
                        return permissions.Any(p => p.ScreenEdit);
                    case ScreenPermission.Delete:
                        return permissions.Any(p => p.ScreenDelete);
                    case ScreenPermission.Print:
                        return permissions.Any(p => p.ScreenPrint);
                    default:
                        return false;
                }
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { Success = false, Message = "Unauthorized access." }
                };
                filterContext.HttpContext.Response.StatusCode = 403;
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
