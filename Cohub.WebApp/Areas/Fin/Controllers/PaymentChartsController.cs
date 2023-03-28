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
    [Route("fin/payment-charts")]
    public class PaymentChartsController : Controller
    {
        private readonly ILogger<PaymentChartsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public PaymentChartsController(ILogger<PaymentChartsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term)
        {
            IQueryable<PaymentChart> query = db.PaymentCharts();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<PaymentChart>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.CategoryId, pattern) ||
                        EF.Functions.ILike(o.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.CategoryId).ThenBy(o => o.FrequencyId);

            var list = await query.ToListAsync();

            return View(list);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new PaymentChart
            {
                IsActive = true,
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(PaymentChart input)
        {
            if (ModelState.IsValid)
            {
                var paymentChart = new PaymentChart(input);
                db.Add(paymentChart);

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = paymentChart.Id }));
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
            var paymentChart = await db.PaymentCharts(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            return View(paymentChart);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var paymentChart = await db.PaymentCharts(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            return View(paymentChart);
        }

        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, PaymentChart input)
        {
            var paymentChart = await db.PaymentCharts(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            paymentChart.UpdateWith(input);

            if (ModelState.IsValid)
            {

                try
                {
                    await db.SaveChangesAsync();

                    // Alert success

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = paymentChart.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(paymentChart);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var paymentChart = await db.PaymentCharts(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(paymentChart);
                db.Comment($"Deleted payment chart {paymentChart}.");

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

            return View(paymentChart);
        }


        [HttpGet("data/options")]
        public async Task<IActionResult> DataOptions(string term, int top = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(Enumerable.Empty<object>());
            }

            IQueryable<PaymentChart> query = db.PaymentCharts();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<PaymentChart>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.CategoryId, pattern) ||
                        EF.Functions.ILike(o.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => o.Id);

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
    }
}
