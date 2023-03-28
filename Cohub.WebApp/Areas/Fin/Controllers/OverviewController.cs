using Cohub.Data;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin")]
    public class OverviewController : Controller
    {
        private readonly CohubDbContext db;

        public OverviewController(CohubDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
