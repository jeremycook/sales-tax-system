using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Lic;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.ReviewLicenses)]
    [Area("Lic")]
    [Route("lic/licenses")]
    public class LicensesController : Controller
    {
        private readonly ILogger<LicensesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public LicensesController(ILogger<LicensesController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<License> query = db.Licenses().IncludeReferences();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var predicate = PredicateBuilder.New<License>();
                term.Split(' ').ToList().ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Title, pattern) ||
                        EF.Functions.ILike(o.Description, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderByDescending(o => o.Created);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<IActionResult> Create(string organizationId)
        {
            var settings = await db.LicenseSettingsAsync();

            var input = new License
            {
                Title = organizationId,
                OrganizationId = organizationId,
                TypeId = LicenseTypeId.Business,
                IssuedDate = DateTime.Today,
                ExpirationDate = settings.CurrentBusinessLicenseExpirationDate,
            };

            return View(input);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(License input, [FromQuery] string organizationId)
        {

            if (ModelState.IsValid)
            {
                db.Comment($"Created license {input}.", new OrganizationComment(input.OrganizationId));

                var license = new License(input);
                db.Add(license);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Created license.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = license.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            return View(license);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            return View(license);
        }


        [Authorize(Policy.Process)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, [FromForm] License input)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (license.IssuedDate != input.IssuedDate)
                {
                    db.Comment($"Changed license {license} start date from {license.IssuedDate:d} to {input.IssuedDate:d}.", new OrganizationComment(input.OrganizationId));
                }
                if (license.ExpirationDate != input.ExpirationDate)
                {
                    db.Comment($"Changed license {license} expiration date from {license.ExpirationDate:d} to {input.ExpirationDate:d}.", new OrganizationComment(input.OrganizationId));
                }

                license.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Updated license.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = license.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/renew-business-license")]
        public async Task<IActionResult> RenewBusinessLicense(int id)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            return View(license);
        }


        [Authorize(Policy.Process)]
        [HttpPost("{id}/renew-business-license")]
        [ActionName(nameof(RenewBusinessLicense))]
        public async Task<IActionResult> RenewBusinessLicensePost(int id)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            var settings = await db.LicenseSettingsAsync();

            if (license.TypeId != LicenseTypeId.Business)
            {
                ModelState.AddModelError("", $"This action can only renew business licenses. This is a {license.Type} license.");
            }
            else if (license.ExpirationDate >= settings.NextBusinessLicenseExpirationDate)
            {
                ModelState.AddModelError("", $"This license has already been renewed based on its expiration date of {license.ExpirationDate:d}.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Change the organization's status to active if not already active
                    if (license.Organization.StatusId != OrganizationStatusId.Active)
                    {
                        db.Comment($"Changed {license.OrganizationId}'s status from {license.Organization.StatusId} to {OrganizationStatusId.Active}.", new OrganizationComment(license.OrganizationId));
                        license.Organization.StatusId = OrganizationStatusId.Active;
                    }

                    // Allow online filing if not already allowed
                    if (!license.Organization.OnlineFiler)
                    {
                        db.Comment($"Changed {license.OrganizationId}'s online filer flag to on.", new OrganizationComment(license.OrganizationId));
                        license.Organization.OnlineFiler = true;
                    }

                    // Set the business license's expiration date to the "next business license expiration date"
                    db.Comment($"Renewed {license.OrganizationId}'s business license {license}, changing expiration date from {license.ExpirationDate:d} to {settings.NextBusinessLicenseExpirationDate:d}.", new OrganizationComment(license.OrganizationId));
                    license.ExpirationDate = settings.NextBusinessLicenseExpirationDate;

                    await db.SaveChangesAsync();


                    TempData.Success("Renewed license.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = license.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(license);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(license);
                db.Comment($"Deleted license {license}.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted license.");

                    return Redirect(Url.ReturnUrlOrAction("Index"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(license);
        }


        [HttpGet("{id}/print")]
        public async Task<IActionResult> Print(int id, bool autoprint = true)
        {
            var license = await db.Licenses(id).SingleOrDefaultAsync();

            if (license == null)
            {
                return NotFound();
            }

            ViewBag._Autoprint = autoprint;
            return View(license);
        }


        [Authorize(Policy.Process)]
        [HttpGet("data/options")]
        public async Task<IActionResult> DataOptions(string term, int top = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(Enumerable.Empty<object>());
            }

            var query = db.Set<License>()
                .Where(o =>
                    EF.Functions.ILike(o.Title, $"%{term}%")
                )
                .OrderBy(o => o.Id);

            var list = await query
                .Take(top)
                .Select(o => new
                {
                    Value = o.Id,
                    Label = o.Title,
                    Selected = o.Title == term || o.ToString() == term,
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
