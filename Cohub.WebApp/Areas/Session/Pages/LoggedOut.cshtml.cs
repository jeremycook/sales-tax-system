using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SiteKit.Session.UI.Areas.Session.Pages
{
    [AllowAnonymous]
    public class LoggedOutModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("Logout");
            }
            else
            {
                return Page();
            }
        }
    }
}
