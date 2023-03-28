using Cohub.Data;
using Cohub.Data.Lic;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Admin.Controllers
{
    [Authorize(Policy.Manage)]
    [Area("lic")]
    [Route("lic/license-settings")]
    public class LicenseSettingsController : Controller
    {
        private readonly ILogger<LicenseSettingsController> logger;
        private readonly CohubDbContext db;

        public LicenseSettingsController(ILogger<LicenseSettingsController> logger, CohubDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var settings = await db.LicenseSettingsAsync();

            return View(settings);
        }


        [HttpGet("edit")]
        public async Task<IActionResult> Edit()
        {
            var settings = await db.LicenseSettingsAsync();

            return View(settings);
        }


        [HttpPost("edit")]
        public async Task<IActionResult> Edit([FromForm] LicenseSettings input)
        {
            var settings = await db.LicenseSettingsAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    if (input != settings)
                    {
                        db.Add(input);
                        await db.SaveChangesAsync();
                    }

                    TempData.Success("Updated license settings.");

                    return Redirect(Url.ReturnUrlOrAction("Details"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }
    }
}
