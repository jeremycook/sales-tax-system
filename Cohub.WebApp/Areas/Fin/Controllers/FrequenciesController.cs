using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
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
    [Route("fin/frequencies")]
    public class FrequenciesController : Controller
    {
        private readonly ILogger<FrequenciesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public FrequenciesController(ILogger<FrequenciesController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term)
        {
            IQueryable<Frequency> query = db.Frequencies();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Frequency>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Description, pattern) ||
                        o.Periods!.Any(p => EF.Functions.ILike(p.Id, pattern)) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.Id);

            var list = await query.ToListAsync();

            return View(list);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new Frequency
            {
                IsActive = true,
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Frequency input)
        {
            if (ModelState.IsValid)
            {
                var frequency = new Frequency(input);
                db.Add(frequency);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Frequency created.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = frequency.Id }));
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
            var frequency = await db.Frequencies(id).SingleOrDefaultAsync();

            if (frequency == null)
            {
                return NotFound();
            }

            return View(frequency);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var frequency = await db.Frequencies(id).SingleOrDefaultAsync();

            if (frequency == null)
            {
                return NotFound();
            }

            return View(frequency);
        }


        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(string id, Frequency input)
        {
            var frequency = await db.Frequencies(id).SingleOrDefaultAsync();

            if (frequency == null)
            {
                return NotFound();
            }

            frequency.UpdateWith(input);

            if (ModelState.IsValid)
            {
                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Frequency updated.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = frequency.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(frequency);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var frequency = await db.Frequencies()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (frequency == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(frequency);
                db.Comment($"Deleted frequency {frequency}.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Frequency deleted.");

                    return Redirect(Url.ReturnUrlOrAction("Index"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(frequency);
        }
    }
}
