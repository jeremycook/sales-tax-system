using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Fin.Views.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/transactions")]
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public TransactionsController(ILogger<TransactionsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<ActionResult> Create(int batchId)
        {
            Batch? batch = await db.Batches(batchId).SingleOrDefaultAsync();

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} batch cannot be modified.");
            }

            Transaction? lastTransaction = batch?.Transactions!.OrderBy(o => o.Id).LastOrDefault();

            var transaction = new Transaction
            {
                BatchId = batchId,
            };

            return View(transaction);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromForm] Transaction input)
        {
            var batch = await db.Batches(input.BatchId).SingleOrDefaultAsync();

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} batch cannot be modified.");
            }

            var transaction = new Transaction(input);

            if (ModelState.IsValid)
            {
                db.Add(transaction);

                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = transaction.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(transaction);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var transaction = await db.Transactions(id).SingleOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [Authorize(Policy.Process)]
        [HttpPost("{id}")]
        public async Task<ActionResult> Details(int id, Transaction input)
        {
            var transaction = await db.Transactions(id).SingleOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            var batch = transaction.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} batch cannot be modified.");
            }

            if (ModelState.IsValid)
            {
                transaction.UpdateWith(input);
                transaction.Details!.UpdateWithRange(input.Details ?? Enumerable.Empty<TransactionDetail>(),
                    (s, t) => s.Id == t.Id,
                    (s, t) => t.UpdateWith(s));

                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = transaction.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(transaction);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var transaction = await db.Transactions(id).SingleOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            var batch = transaction.Batch;

            if (batch?.CanModify() == false)
            {
                ModelState.AddModelError("", $"The {batch} batch cannot be modified.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                db.Remove(transaction);

                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", "Batches", new { id = transaction.BatchId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(transaction);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/reverse")]
        public async Task<ActionResult> Reverse(ReverseTransaction input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
                input.OpenRelatedReturns = true;
            }

            var transaction = await db.Transactions(input.Id).SingleOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound();
            }

            if (!transaction.Batch.IsPosted)
            {
                ModelState.AddModelError("", $"The {transaction.Batch} batch has not been posted. Only transactions of posted batches can be reversed.");
            }

            Batch? destinationBatch;
            if (input.BatchId is not null)
            {
                destinationBatch = await db.Batches().IncludeTransactions().SingleOrDefaultAsync(o => o.Id == input.BatchId);
                if (destinationBatch is null)
                {
                    ModelState.AddModelError(nameof(input.BatchId), $"The destination batch was not found. Please select another batch or the option to create a new batch.");
                }
                else if (destinationBatch.IsPosted)
                {
                    ModelState.AddModelError(nameof(input.BatchId), $"The {destinationBatch} destination batch has been posted. The destination batch must be unposted.");
                }
            }
            else
            {
                destinationBatch = null;
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                if (destinationBatch is null)
                {
                    destinationBatch = new Batch
                    {
                        Name = await db.Batches().NextNameAsync(actor.Initials ?? actor.UserId.ToString()),
                        DepositControlAmount = -transaction.Deposited ?? 0,
                        Transactions = new List<Transaction>(),
                        Note = "Reversal batch",
                    };
                    db.Add(destinationBatch);
                }

                var newTransaction = new Transaction
                {
                    Note = $"Reversal of the {transaction} transaction.",
                    Details = new List<TransactionDetail>(),
                };
                destinationBatch.Transactions.Add(newTransaction);
                foreach (var detail in transaction.Details)
                {
                    var newDetail = new TransactionDetail
                    {
                        BucketId = detail.BucketId,
                        CategoryId = detail.CategoryId,
                        SubcategoryId = detail.SubcategoryId,
                        OrganizationId = detail.OrganizationId,
                        PeriodId = detail.PeriodId,
                        EffectiveDate = detail.EffectiveDate,
                        Amount = -detail.Amount,
                        Note = null, // Intentionally not copying the note
                    };
                    newTransaction.Details.Add(newDetail);
                }

                if (input.NSFFee is not null)
                {
                    foreach (var deposit in transaction.Details.Where(o => o.BucketId == BucketId.Deposit))
                    {
                        newTransaction.Details.Add(TransactionDetail.NSFFeeDue(deposit.OrganizationId, deposit.PeriodId, deposit.EffectiveDate, input.NSFFee.Value));
                    }
                }

                if (input.OpenRelatedReturns)
                {
                    var relatedReturns = await (
                        from t in db.TransactionDetails()
                        where t.TransactionId == transaction.Id
                        from r in db.Set<Return>()
                        where
                            r.CategoryId == t.CategoryId &&
                            r.OrganizationId == t.OrganizationId &&
                            r.PeriodId == t.PeriodId
                        select new
                        {
                            Return = r,
                            Period = r.Period,
                        }
                    ).ToArrayAsync();
                    foreach (var item in relatedReturns)
                    {
                        if (item.Return.StatusId == ReturnStatusId.Closed)
                        {
                            item.Return.StatusId = item.Period.IsDue() ? ReturnStatusId.Due : ReturnStatusId.Payable;
                            db.Comment($"Opened return {item.Return}.", new ReturnComment(item.Return), new OrganizationComment(item.Return.OrganizationId), new BatchComment(destinationBatch));
                        }
                    }
                }

                try
                {
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", "Batches", new { id = destinationBatch.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", "Exception: " + ex.OriginalMessage());
                }
            }

            return View(input);
        }
    }
}
