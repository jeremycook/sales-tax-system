using Cohub.Data;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Info;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Manage)]
    [Area("Org")]
    [Route("org/labels")]
    public class LabelsController : Controller
    {
        private readonly ILogger<LabelsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public LabelsController(ILogger<LabelsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term)
        {
            IQueryable<Label> query = db.Labels();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var predicate = PredicateBuilder.New<Label>();
                term.Split(' ').ToList().ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Title, pattern) ||
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
            var input = new Label
            {
                IsActive = true,
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Label input)
        {
            if (ModelState.IsValid)
            {
                var label = new Label(input);
                db.Add(label);
                db.Comment($"Created Label {label}.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Created label.");

                    return RedirectToAction("Details", new { id = label.Id });
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
            var label = await db.Labels(id).SingleOrDefaultAsync();

            if (label == null)
            {
                return NotFound();
            }

            return View(label);
        }


        [Route("{id}/edit")]
        public async Task<IActionResult> Edit(Label input)
        {
            var label = await db.Labels(input.Id).SingleOrDefaultAsync();

            if (Request.IsGet())
            {
                ModelState.Clear();
                input = label;
            }

            if (label == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    label.UpdateWith(input);
                    await db.SaveChangesAsync();

                    TempData.Success("Updated label.");

                    return RedirectToAction("Details", new { id = input.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var label = await db.Labels()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (label == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(label);
                db.Comment($"Deleted {label} label.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted label.");

                    return RedirectToAction("Index", new { sk_intent = "zoom" });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(label);
        }
    }
}
