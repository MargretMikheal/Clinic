using System.Web.Mvc;

namespace Clinic.Web.Areas.Auth.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
