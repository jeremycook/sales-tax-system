using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteKit.Session.UI.Areas.Session.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public ActionResult OnGet(string returnUrl = "")
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith(Url.Page("Login")) ? returnUrl : Url.Content("~/")
            });
        }
    }
}
