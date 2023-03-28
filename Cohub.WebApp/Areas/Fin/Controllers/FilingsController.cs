using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/filings")]
    public class FilingsController : Controller
    {
        private readonly ILogger<FilingsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public FilingsController(ILogger<FilingsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Filing> query = db.Filings()
                .IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Filing>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string contains = $"%{token}%";
                    string startswith = $"{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Return.Status.Name, token) ||
                        EF.Functions.ILike(o.Return.OrganizationId, contains) ||
                        EF.Functions.ILike(o.Return.CategoryId, startswith) ||
                        EF.Functions.ILike(o.Return.PeriodId, startswith) ||
                        EF.Functions.ILike(o.Return.Organization.OrganizationName, contains) ||
                        EF.Functions.ILike(o.Return.Organization.Dba, contains) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderByDescending(o => o.Created);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Tokenizer = tokenizer;
            ViewBag._Paging = paging;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<IActionResult> Create(int returnId)
        {
            var ret = await db.Returns(returnId).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            Filing input = Filing.Activate(ret);

            ViewBag._Title = $"New Filing for {ret}";
            return View(input);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(int returnId, IFormCollection _)
        {
            var ret = await db.Returns(returnId).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            Filing input = Filing.Activate(ret);

            if (await TryUpdateModelAsync(input, input.GetType(), string.Empty) && ModelState.IsValid)
            {
                ret.HasFiled = true;
                db.Add(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success($"Created filing.");

                    return this.RedirectToReturnUrlOrAction("Details", new { id = input.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            ViewBag._Title = $"New Filing for {ret}";
            return View(input);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var filing = await db.Filings(id).SingleOrDefaultAsync();

            if (filing == null)
            {
                return NotFound();
            }

            return View(filing);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var filing = await db.Filings(id).SingleOrDefaultAsync();

            if (filing == null)
            {
                return NotFound();
            }

            return View(filing);
        }

        [Authorize(Policy.Process)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, IFormCollection _)
        {
            var filing = await db.Filings(id).SingleOrDefaultAsync();

            if (filing == null)
            {
                return NotFound();
            }

            var ret = await db.Returns(filing.ReturnId).SingleOrDefaultAsync();
            Filing input = Filing.Activate(ret);

            if (await TryUpdateModelAsync(input, input.GetType(), string.Empty) && ModelState.IsValid)
            {
                filing.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success($"Saved return.");

                    return RedirectToAction("Details", new { id = filing.Id });
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
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var filing = await db.Filings(id).SingleOrDefaultAsync();

            if (filing == null)
            {
                return NotFound();
            }

            var @return = filing.Return;

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Comment($"Deleted filing {filing}.", new OrganizationComment(@return.OrganizationId));
                db.Remove(filing);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success($"Deleted filing.");

                    return RedirectToAction("Details", "Returns", new { id = @return.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(filing);
        }
    }
}
