using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/transaction-details")]
    public class TransactionDetailsController : Controller
    {
        private readonly ILogger<TransactionDetailsController> logger;
        private readonly CohubDbContext db;

        public TransactionDetailsController(ILogger<TransactionDetailsController> logger, CohubDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<ActionResult> Create(int transactionId, bool isPayment = false)
        {
            var transaction = await db.Set<Transaction>()
                .Include(o => o.Batch)
                .Include(o => o.Details)
                .SingleOrDefaultAsync(o => o.Id == transactionId);

            if (transaction is null)
            {
                return NotFound();
            }

            var batch = transaction.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} cannot be modified.");
            }

            TransactionDetail transactionDetail = transaction.Details!.OrderBy(o => o.Id).LastOrDefault() is TransactionDetail td ?
                new TransactionDetail(td)
                {
                    Amount = 0,
                } :
                new TransactionDetail
                {
                    SubcategoryId = SubcategoryId.Net,
                    EffectiveDate = DateTime.Today,
                };

            if (isPayment)
            {
                throw new NotImplementedException();
                //transactionDetail.BucketId = BucketId.CPay;
                //transactionDetail.Amount = (transaction.NonPaymentSum ?? 0) - (transaction.PaymentSum ?? 0);
            }

            return View(transactionDetail);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<ActionResult> Create(int transactionId, [FromForm] TransactionDetail input, bool calculateCredits)
        {
            var transaction = await db.Set<Transaction>()
                .Include(o => o.Batch)
                .Include(o => o.Details)
                .SingleOrDefaultAsync(o => o.Id == transactionId);

            if (transaction is null)
            {
                return NotFound();
            }

            var batch = transaction.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} cannot be modified.");
            }

            if (ModelState.IsValid)
            {
                var transactionDetail = new TransactionDetail(input);
                transaction.Details!.Add(transactionDetail);

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Created transaction detail.");
                    return RedirectToAction("Details", "Transactions", new { id = transactionDetail.TransactionId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var transactionDetail = await db.Set<TransactionDetail>()
                .Include(o => o.Transaction).ThenInclude(o => o!.Batch)
                .Include(o => o.Bucket)
                .Include(o => o.Category)
                .Include(o => o.Period)
                .Include(o => o.Organization)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (transactionDetail == null)
            {
                return NotFound();
            }

            return View(transactionDetail);
        }

        [Authorize(Policy.Process)]
        [HttpPost("{id}")]
        public async Task<ActionResult> Details(int id, TransactionDetail input)
        {
            var transactionDetail = await db.Set<TransactionDetail>()
                .Include(o => o.Transaction).ThenInclude(o => o!.Batch)
                .Include(o => o.Bucket)
                .Include(o => o.Category)
                .Include(o => o.Period)
                .Include(o => o.Organization)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (transactionDetail == null)
            {
                return NotFound();
            }

            var batch = transactionDetail.Transaction!.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} cannot be modified.");
            }

            if (ModelState.IsValid)
            {
                transactionDetail.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Saved changes.");
                    return RedirectToAction("Details", "Transactions", new { id = transactionDetail.TransactionId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var transactionDetail = await db.Set<TransactionDetail>()
                .Include(o => o.Transaction).ThenInclude(o => o!.Batch)
                .SingleOrDefaultAsync(o => o.Id == id);

            if (transactionDetail == null)
            {
                return NotFound();
            }

            var batch = transactionDetail.Transaction!.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} cannot be modified.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                db.Remove(transactionDetail);

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Deleted transaction detail.");
                    return RedirectToAction("Details", "Transactions", new { id = transactionDetail.TransactionId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(transactionDetail);
        }

    }
}
