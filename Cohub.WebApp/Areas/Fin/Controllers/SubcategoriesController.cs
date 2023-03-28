using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy.Manage)]
    [Area("Fin")]
    [Route("fin/subcategories")]
    public class SubcategoriesController : Controller
    {
        private readonly ILogger<SubcategoriesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public SubcategoriesController(ILogger<SubcategoriesController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Subcategory> query = db.Subcategories();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Subcategory>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.Id);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new Subcategory
            {
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Subcategory input)
        {
            if (ModelState.IsValid)
            {
                var subcategory = new Subcategory(input);
                db.Add(subcategory);

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = subcategory.Id }));
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
            var subcategory = await db.Subcategories(id).SingleOrDefaultAsync();

            if (subcategory == null)
            {
                return NotFound();
            }

            return View(subcategory);
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> Details(string id, Subcategory input)
        {
            var subcategory = await db.Subcategories(id).SingleOrDefaultAsync();

            if (subcategory == null)
            {
                return NotFound();
            }

            subcategory.UpdateWith(input);

            if (ModelState.IsValid)
            {

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = subcategory.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(subcategory);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var subcategory = await db.Subcategories()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (subcategory == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(subcategory);
                db.Comment($"Deleted subcategory {subcategory}.");

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Index"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(subcategory);
        }
    }
}
