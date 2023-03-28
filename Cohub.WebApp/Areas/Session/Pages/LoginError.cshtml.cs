using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteKit.Session.UI.Areas.Session.Pages
{
    [AllowAnonymous]
    public class LoginErrorModel : PageModel
    {
        public void OnGet(string returnUrl = "")
        {
            // Clean up framework cookies in case any are causing problems
            foreach (var cookie in Request.Cookies)
            {
                if (cookie.Key.StartsWith(".AspNetCore."))
                {
                    Response.Cookies.Delete(cookie.Key);
                }
            }

            ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        }
    }
}
