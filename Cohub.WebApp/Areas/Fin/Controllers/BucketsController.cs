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
    [Route("fin/buckets")]
    public class BucketsController : Controller
    {
        private readonly ILogger<BucketsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public BucketsController(ILogger<BucketsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Bucket> query = db.Buckets();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Bucket>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Name, pattern) ||
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
            var input = new Bucket
            {
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Bucket input)
        {
            if (ModelState.IsValid)
            {
                var bucket = new Bucket(input);
                db.Add(bucket);

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = bucket.Id }));
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
            var bucket = await db.Buckets(id).SingleOrDefaultAsync();

            if (bucket == null)
            {
                return NotFound();
            }

            return View(bucket);
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> Details(string id, Bucket input)
        {
            var bucket = await db.Buckets(id).SingleOrDefaultAsync();

            if (bucket == null)
            {
                return NotFound();
            }

            bucket.UpdateWith(input);

            if (ModelState.IsValid)
            {

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = bucket.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(bucket);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var bucket = await db.Buckets()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (bucket == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(bucket);
                db.Comment($"Deleted bucket {bucket}.");

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

            return View(bucket);
        }
    }
}
