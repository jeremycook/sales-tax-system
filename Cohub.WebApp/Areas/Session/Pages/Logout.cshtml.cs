using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteKit.Session.UI.Areas.Session.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult OnPost()
        {
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
