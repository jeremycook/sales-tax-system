using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteKit.Session.UI.Areas.Session.Pages
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public void OnGet(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
        }
    }
}
