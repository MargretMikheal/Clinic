using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;

[assembly: OwinStartup(typeof(Clinic.Web.Startup))]

namespace Clinic.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Auth/Account/Login"),
                CookieHttpOnly = true,
                CookieSecure = CookieSecureOption.SameAsRequest,
                ExpireTimeSpan = TimeSpan.FromHours(8),
                SlidingExpiration = true
            });
        }
    }
}
