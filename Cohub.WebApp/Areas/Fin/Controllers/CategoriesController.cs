using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Org;
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
    [Route("fin/categories")]
    public class CategoriesController : Controller
    {
        private readonly ILogger<CategoriesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public CategoriesController(ILogger<CategoriesController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Category> query = db.Categories();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Category>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Description, pattern) ||
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
            var input = new Category
            {
                IsActive = true,
                TypeId = CategoryTypeId.Tax,
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Category input)
        {
            if (ModelState.IsValid)
            {
                var category = new Category(input);
                db.Add(category);

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = category.Id }));
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
            var category = await db.Categories(id).SingleOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var category = await db.Categories(id).SingleOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(string id, Category input)
        {
            var category = await db.Categories(id).SingleOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            category.UpdateWith(input);

            if (ModelState.IsValid)
            {
                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = category.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(category);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await db.Categories()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(category);
                db.Comment($"Deleted category {category}.");

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

            return View(category);
        }
    }
}
