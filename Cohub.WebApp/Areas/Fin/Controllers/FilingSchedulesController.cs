using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Humanizer;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Fin")]
    [Route("fin/filing-schedules")]
    public class FilingSchedulesController : Controller
    {
        private readonly ILogger<FilingSchedulesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;
        private readonly ReturnGenerator returnGenerator;

        public FilingSchedulesController(ILogger<FilingSchedulesController> logger, Actor actor, CohubDbContext db, ReturnGenerator returnGenerator)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
            this.returnGenerator = returnGenerator;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<FilingSchedule> query = db.FilingSchedules().IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<FilingSchedule>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.OrganizationId, pattern) ||
                        EF.Functions.ILike(o.PaymentChart.Name, pattern) ||
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


        [Route("create")]
        public async Task<IActionResult> Create(FilingSchedule input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();

                input.StartDate = DateTime.Today;
                input.EndDate = DateTime.MaxValue.Date;

                if (!input.OrganizationId.IsNullOrWhiteSpace())
                {
                    var latestFilingSchedule = await db.Set<FilingSchedule>()
                        .Where(o => o.OrganizationId == input.OrganizationId)
                        .OrderByDescending(o => o.StartDate)
                        .FirstOrDefaultAsync();

                    if (latestFilingSchedule is not null)
                    {
                        input.PaymentChartId = latestFilingSchedule.PaymentChartId;
                        if (DateTime.MinValue.Date < latestFilingSchedule.EndDate && latestFilingSchedule.EndDate < DateTime.MaxValue.Date)
                        {
                            input.StartDate = latestFilingSchedule.EndDate.AddDays(1);
                        }
                        else
                        {
                            input.StartDate = DateTime.MaxValue.Date;
                            input.EndDate = DateTime.MaxValue.Date;
                        }
                    }
                }
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    var filingSchedule = new FilingSchedule(input);
                    db.Add(filingSchedule);
                    db.Comment($"Created filing schedule {input}.", new OrganizationComment(input.OrganizationId));

                    await db.SaveChangesAsync();
                    TempData.Success("Created filing schedule.");

                    var generatedReturns = await returnGenerator.GenerateMissingReturnsAsync(new[] { filingSchedule.OrganizationId });
                    if (generatedReturns.Any())
                    {
                        TempData.Success($"Generated {generatedReturns.Count} returns.");
                    }

                    return RedirectToAction("Details", "Organizations", new { area = "Org", id = filingSchedule.OrganizationId });
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
            var filingSchedule = await db.FilingSchedules(id).SingleOrDefaultAsync();

            if (filingSchedule == null)
            {
                return NotFound();
            }

            return View(filingSchedule);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var filingSchedule = await db.FilingSchedules(id).SingleOrDefaultAsync();

            if (filingSchedule == null)
            {
                return NotFound();
            }

            return View(filingSchedule);
        }


        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(FilingSchedule input)
        {
            var filingSchedule = await db.FilingSchedules(input.Id).SingleOrDefaultAsync();

            if (filingSchedule == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        if (filingSchedule.PaymentChartId != input.PaymentChartId)
                        {
                            db.Comment($"Changed filing schedule {filingSchedule} payment chart from {filingSchedule.PaymentChartId} to {input.PaymentChartId}.", new OrganizationComment(input.OrganizationId));
                        }
                        if (filingSchedule.StartDate != input.StartDate)
                        {
                            db.Comment($"Changed filing schedule {filingSchedule} starting from {filingSchedule.StartDate:d} to {input.StartDate:d}.", new OrganizationComment(input.OrganizationId));
                        }
                        if (filingSchedule.EndDate != input.EndDate)
                        {
                            db.Comment($"Changed filing schedule {filingSchedule} ending from {filingSchedule.EndDate:d} to {input.EndDate:d}.", new OrganizationComment(input.OrganizationId));
                        }

                        filingSchedule.UpdateWith(input);
                        await db.SaveChangesAsync();

                        var deletedReturns = await DeleteReturnsWithoutAFilingScheduleAsync(new[] { filingSchedule.OrganizationId });

                        var generatedReturns = await returnGenerator.GenerateMissingReturnsAsync(new[] { filingSchedule.OrganizationId });

                        await tx.CommitAsync();

                        TempData.Success("Updated filing schedule.");
                        if (deletedReturns.Any())
                        {
                            TempData.Success($"Removed {deletedReturns.Count} payable, unfiled returns.");
                        }
                        if (generatedReturns.Any())
                        {
                            TempData.Success($"Generated {generatedReturns.Count} returns.");
                        }
                    }

                    return RedirectToAction("Details", "Organizations", new { area = "Org", id = filingSchedule.OrganizationId });
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
            var filingSchedule = await db.FilingSchedules(id).SingleOrDefaultAsync();

            if (filingSchedule == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        db.Comment($"Deleted filing schedule {filingSchedule}.", new OrganizationComment(filingSchedule.OrganizationId));
                        db.Remove(filingSchedule);
                        await db.SaveChangesAsync();

                        var deletedReturns = await DeleteReturnsWithoutAFilingScheduleAsync(new[] { filingSchedule.OrganizationId });

                        await tx.CommitAsync();

                        TempData.Success("Deleted filing schedule.");
                        if (deletedReturns.Any())
                        {
                            TempData.Success($"Removed {deletedReturns.Count} payable, unfiled returns.");
                        }
                    }

                    return RedirectToAction("Details", "Organizations", new { area = "Org", id = filingSchedule.OrganizationId });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(filingSchedule);
        }


        [HttpGet("data/options")]
        public async Task<IActionResult> DataOptions(string term, int top = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(Enumerable.Empty<object>());
            }

            IQueryable<FilingSchedule> query = db.Set<FilingSchedule>();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<FilingSchedule>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.OrganizationId, pattern) ||
                        EF.Functions.ILike(o.PaymentChart.Name, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => o.OrganizationId)
                .ThenBy(o => o.PaymentChartId)
                .ThenBy(o => o.StartDate);

            var list = await query
                .Take(top)
                .Select(o => new
                {
                    Value = o.Id,
                    Label = o.ToString(),
                })
                .ToListAsync();

            return Ok(list);
        }

        private async Task<List<Return>> DeleteReturnsWithoutAFilingScheduleAsync(IEnumerable<string> organizationIds)
        {
            // Remove this organization's payable, unfiled returns that don't have a matching filing schedule
            var returns = await db.Returns()
                .Where(o =>
                    o.StatusId == ReturnStatusId.Payable &&
                    organizationIds.Contains(o.OrganizationId) &&
                    !o.Filings.Any() &&
                    !db.FilingSchedules().Any(fs =>
                        organizationIds.Contains(fs.OrganizationId) &&
                        fs.PaymentChart.CategoryId == o.CategoryId &&
                        fs.PaymentChart.FrequencyId == o.Period.FrequencyId &&
                        o.Period.StartDate <= fs.EndDate && o.Period.EndDate >= fs.StartDate
                    )
                )
                .ToListAsync();
            foreach (var @return in returns)
            {
                db.Comment($"Removed the payable, unfiled {@return} return.", new OrganizationComment(@return.OrganizationId));
                db.Returns().Remove(@return);
            }
            await db.SaveChangesAsync();
            return returns;
        }
    }
}
