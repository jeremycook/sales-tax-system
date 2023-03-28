using Microsoft.AspNetCore.Mvc;
using System;

namespace SiteKit.AspNetCore.Views.Shared.Components
{
    public class DateTimeOffsetViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DateTimeOffset? moment)
        {
            return View(moment);
        }
    }
}
