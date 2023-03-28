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
    [Route("fin/payment-configurations")]
    public class PaymentConfigurationsController : Controller
    {
        private readonly ILogger<PaymentConfigurationsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public PaymentConfigurationsController(ILogger<PaymentConfigurationsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term)
        {
            IQueryable<PaymentConfiguration> query = db.PaymentConfigurations().IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<PaymentConfiguration>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.PaymentChart.Name, pattern) ||
                        EF.Functions.ILike(o.PaymentChart.CategoryId, pattern) ||
                        EF.Functions.ILike(o.PaymentChart.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.PaymentChart.CategoryId).ThenBy(o => o.PaymentChart.FrequencyId);

            var list = await query.ToListAsync();

            return View(list);
        }


        [HttpGet("create")]
        public IActionResult Create(PaymentConfiguration input)
        {
            if (input.StartDate == DateTime.MinValue)
            {
                input.StartDate = DateTime.Today;
            }

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(PaymentConfiguration input)
        {
            if (ModelState.IsValid)
            {
                var paymentChart = new PaymentConfiguration(input);
                db.Add(paymentChart);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Payment Configuration created.");

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
            var paymentChart = await db.PaymentConfigurations(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            return View(paymentChart);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var paymentChart = await db.PaymentConfigurations(id).SingleOrDefaultAsync();

            if (paymentChart == null)
            {
                return NotFound();
            }

            return View(paymentChart);
        }

        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, PaymentConfiguration input)
        {
            var paymentChart = await db.PaymentConfigurations(id).SingleOrDefaultAsync();

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

                    TempData.Success("Payment Configuration updated.");

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
            var paymentChart = await db.PaymentConfigurations(id).SingleOrDefaultAsync();

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

                    TempData.Success("Payment Configuration deleted.");

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
    }
}
