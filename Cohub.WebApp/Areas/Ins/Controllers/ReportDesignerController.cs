using Cohub.Data;
using Cohub.Data.Ins;
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

namespace Cohub.WebApp.Areas.Ins.Controllers
{
    [Authorize(Policy.Super)]
    [Area("Ins")]
    [Route("ins/report-designer")]
    public class ReportDesignerController : Controller
    {
        private readonly ILogger<ReportDesignerController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public ReportDesignerController(ILogger<ReportDesignerController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Report> report = db.Set<Report>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var predicate = PredicateBuilder.New<Report>();
                term.Split(' ').ToList().ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.QueryDefinitionId, pattern) ||
                        EF.Functions.ILike(o.Template, pattern) ||
                        false
                    );
                });
                report = report.AsExpandableEFCore().Where(predicate);
            }

            report = report.OrderBy(o => o.Id);

            paging.TotalRecords = await report.CountAsync();
            var list = await report.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new Report();
            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Report input)
        {
            if (ModelState.IsValid)
            {
                var report = new Report(input);
                db.Add(report);
                db.Comment($"Created Report {report}.");

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return RedirectToAction("Details", new { id = report.Id });
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
            var model = await db.Set<Report>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Details(int id, [FromForm] Report input)
        {
            var model = await db.Set<Report>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                model.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return RedirectToAction("Details", new { id = model.Id });
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
        public async Task<IActionResult> Delete(int id)
        {
            var model = await db.Set<Report>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(model);
                db.Comment($"Deleted {model} report.");

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return RedirectToAction("Index", new { sk_intent = "zoom" });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(model);
        }
    }
}
