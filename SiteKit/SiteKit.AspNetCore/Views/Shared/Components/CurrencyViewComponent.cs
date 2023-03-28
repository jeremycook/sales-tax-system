using Microsoft.AspNetCore.Mvc;

namespace SiteKit.AspNetCore.Views.Shared.Components
{
    public class CurrencyViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(decimal? amount)
        {
            return View(amount);
        }
    }
}
