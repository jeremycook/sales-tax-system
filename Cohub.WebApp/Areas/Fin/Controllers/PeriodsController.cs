using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Fin.Views.Periods;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Manage)]
    [Area("Fin")]
    [Route("fin/periods")]
    public class PeriodsController : Controller
    {
        private readonly ILogger<PeriodsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public PeriodsController(ILogger<PeriodsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term)
        {
            IQueryable<Period> query = db.Periods().IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Period>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.FrequencyId).ThenBy(o => o.StartDate).ThenBy(o => o.DueDate);

            var list = await query.ToListAsync();

            return View(list);
        }


        [HttpGet("create")]
        public async Task<IActionResult> Create(string? frequencyId, bool createAnother = false)
        {
            var input = new CreatePeriod
            {
                FrequencyId = frequencyId ?? string.Empty,
                DueDate = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                CreateAnother = createAnother,
            };

            if (frequencyId != null)
            {
                Period? lastPeriod = await db.Periods().Include(o => o.Frequency)
                    .Where(o => o.FrequencyId == frequencyId)
                    .OrderBy(o => o.StartDate)
                    .LastOrDefaultAsync();
                if (lastPeriod != null)
                {
                    var change = (lastPeriod.EndDate - lastPeriod.StartDate).TotalDays + 1;

                    input.StartDate = lastPeriod.StartDate.AddDays(change);
                    input.EndDate = lastPeriod.EndDate.AddDays(change);
                    input.DueDate = lastPeriod.DueDate.AddDays(change);
                }
            }

            return View(input);
        }

        [HttpPost("create")]
        [ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost(CreatePeriod input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var frequency = await db.Frequencies().FindAsync(input.FrequencyId);

                    Period period = input.Create(frequency);
                    db.Add(period);

                    await db.SaveChangesAsync();

                    TempData.Success("Period created.");

                    if (input.CreateAnother)
                    {
                        return RedirectToAction("Create", new { frequencyId = period.FrequencyId, createAnother = true });
                    }
                    else
                    {
                        return Redirect(Url.ReturnUrlOrAction("Details", new { id = period.Id }));
                    }
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
        public async Task<IActionResult> Details(string id)
        {
            var period = await db.Periods(id).SingleOrDefaultAsync();

            if (period == null)
            {
                return NotFound();
            }

            return View(period);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var period = await db.Periods(id).SingleOrDefaultAsync();

            if (period == null)
            {
                return NotFound();
            }

            return View(period);
        }


        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit([FromRoute] string id, Period input)
        {
            var period = await db.Periods(id).SingleOrDefaultAsync();

            if (period == null)
            {
                return NotFound();
            }

            period.UpdateWith(input);

            if (ModelState.IsValid)
            {

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Period updated.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = period.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(period);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var period = await db.Periods()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (period == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(period);
                db.Comment($"Deleted period {period}.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Period deleted.");

                    return Redirect(Url.ReturnUrlOrAction("Index"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(period);
        }
    }
}
