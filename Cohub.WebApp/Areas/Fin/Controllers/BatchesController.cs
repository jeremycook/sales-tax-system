using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Batches;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Fin.Views.Batches;
using Dapper;
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

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/batches")]
    public class BatchesController : Controller
    {
        private readonly ILogger<BatchesController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public BatchesController(ILogger<BatchesController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Batch> query = db.Batches().IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Batch>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string lower = token.ToLower();
                    string contains = $"%{token}%";
                    string startswith = $"{token}%";
                    string endswith = $"%{token}";
                    predicate = predicate.And(b =>
                        (lower == "posted" && b.IsPosted) ||
                        (lower == "unposted" && !b.IsPosted) ||
                        (lower == "balanced" && b.IsBalanced) ||
                        (lower == "unbalanced" && !b.IsBalanced) ||
                        EF.Functions.ILike(b.Name, contains) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.BucketId, token))) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.OrganizationId, contains))) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.CategoryId, startswith))) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.PeriodId, startswith))) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.Organization.OrganizationName, startswith))) ||
                        b.Transactions.Any(t => t.Details.Any(o => EF.Functions.ILike(o.Organization.Dba, startswith))) ||
                        tokenizer.Numbers.Contains(b.DepositControlAmount) ||
                        b.Transactions.Any(t => t.Deposited != null && tokenizer.Numbers.Contains(t.Deposited.Value)) ||
                        b.Transactions.Any(t => t.Details.Any(td => tokenizer.Numbers.Contains(td.Amount))) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => !o.IsPosted ? 0 : 1)
                .ThenByDescending(o => o.Created);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var input = new Batch
            {
                Name = await db.Batches().NextNameAsync(actor.Initials ?? "BATCH"),
            };

            return View(input);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(Batch input, string? forwardUrl = null)
        {
            if (!input.Name.IsNullOrWhiteSpace() && await db.Batches().AnyAsync(o => o.Name == input.Name))
            {
                ModelState.AddModelError(nameof(input.Name), $"A batch named '{input.Name}' already exists.");
            }

            if (ModelState.IsValid)
            {
                var batch = new Batch(input);

                db.Add(batch);
                db.Comment($"Created batch {batch}.", new BatchComment(batch));

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Created batch.");
                    if (forwardUrl?.Replace("--batchId--", batch.Id.ToString()) is string redirectUrl && Url.IsLocalUrl(redirectUrl))
                    {
                        return LocalRedirect(redirectUrl);
                    }
                    else
                    {
                        return RedirectToAction("Details", new { id = batch.Id });
                    }
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
        [HttpGet("[action]")]
        public async Task<IActionResult> Refund(RefundRequest input)
        {
            ModelState.Clear();

            await RefreshRefundAmountsAsync(input);

            return View(input);
        }

        [Authorize(Policy.Process)]
        [HttpPost("[action]")]
        public async Task<IActionResult> Refund(RefundRequest input, object? _)
        {
            await RefreshRefundAmountsAsync(input);

            if (input.Command == RefundRequestCommand.Reset)
            {
                ModelState.Clear();
            }

            if (input.Command == RefundRequestCommand.Refund)
            {
                var refunds = input.Refunds!;

                int i = 0;
                foreach (var item in refunds)
                {
                    if (item.RefundAmount > item.AvailableAmount)
                    {
                        ModelState.AddModelError($"RefundAmounts[{i}].RefundAmount", "The Refund Amount cannot be greater than the Available Amount.");
                    }
                    i++;
                }

                var revOvr = refunds
                    .Where(o => (new[] { BucketId.Revenue, BucketId.Overpayment }).Contains(o.BucketId))
                    .Sum(o => o.RefundAmount);
                var dep = refunds
                    .Where(o => o.BucketId == BucketId.Deposit)
                    .Sum(o => o.RefundAmount);
                if (dep != revOvr)
                {
                    ModelState.AddModelError("RefundAmounts", $"The total revenue and overpayment refund amount, {revOvr:C}, must equal the total deposit refund amount, {dep:C}.");
                }

                if (revOvr <= 0)
                {
                    ModelState.AddModelError("RefundAmounts", "The total revenue and overpayment refund amount must be greater than $0.");
                }

                if (dep <= 0)
                {
                    ModelState.AddModelError("RefundAmounts", "The total deposit refund amount must be greater than $0.");
                }
            }

            if (input.Command == RefundRequestCommand.Refund &&
                ModelState.IsValid)
            {
                try
                {
                    var refunds = input.Refunds!.Where(o => o.RefundAmount > 0);
                    var totalRefund = refunds.Where(o => o.BucketId == BucketId.Deposit).Sum(o => o.RefundAmount);
                    var batch = new Batch()
                    {
                        Name = await db.Batches().NextNameAsync("REFUND"),
                        DepositControlAmount = -totalRefund,
                        Transactions = new()
                        {
                            new Transaction()
                            {
                                Details = refunds
                                    .Select(o => new TransactionDetail()
                                    {
                                        OrganizationId = input.OrganizationId,
                                        PeriodId = o.PeriodId,
                                        CategoryId = o.CategoryId,
                                        BucketId = o.BucketId,
                                        SubcategoryId = o.SubcategoryId,
                                        Amount = -o.RefundAmount,
                                        EffectiveDate = input.NewEffectiveDate,
                                    })
                                    .ToList()
                            }
                        },
                        Note = $"Refund {totalRefund:C} to {input.OrganizationId}.",
                    };
                    db.Add(batch);
                    batch.Post();
                    db.Comment($"Created batch {batch}.", new BatchComment(batch), new OrganizationComment(input.OrganizationId));

                    await db.SaveChangesAsync();

                    TempData.Success($"Created {batch} batch.");
                    return RedirectToAction("Details", "Batches", new { id = batch.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }
        private async Task RefreshRefundAmountsAsync(RefundRequest input)
        {
            if (input.OrganizationId is null)
            {
                input.Command = RefundRequestCommand.Reset;
                input.Refunds = null;
                return;
            }
            else if (!await db.Organizations().AnyAsync(o => o.Id == input.OrganizationId))
            {
                // Invalid organization ID
                ModelState.AddModelError(nameof(input.OrganizationId), $"The {input.OrganizationId} organization ID did not match an organization.");
                input.Command = RefundRequestCommand.Reset;
                input.OrganizationId = null;
                input.Refunds = null;
                return;
            }

            var refundAmounts = await db.TransactionDetails()
                .Where(o =>
                    new[] { BucketId.Revenue, BucketId.Overpayment, BucketId.Deposit }.Contains(o.BucketId) &&
                    o.OrganizationId == input.OrganizationId &&
                    input.FromEffectiveDate <= o.EffectiveDate && o.EffectiveDate <= input.ThroughEffectiveDate
                )
                .GroupBy(o => new
                {
                    o.PeriodId,
                    o.Period.EndDate,
                    o.CategoryId,
                    o.SubcategoryId,
                    o.BucketId,
                })
                .OrderBy(o => o.Key.EndDate)
                .ThenBy(o => o.Key.CategoryId)
                .ThenBy(o => o.Key.BucketId)
                .ThenBy(o => o.Key.SubcategoryId)
                .Select(g => new RefundRequestAmount
                {
                    PeriodId = g.Key.PeriodId,
                    CategoryId = g.Key.CategoryId,
                    SubcategoryId = g.Key.SubcategoryId,
                    BucketId = g.Key.BucketId,
                    AvailableAmount = g.Sum(o => o.Amount),
                    RefundAmount = 0,
                })
                .Where(o => o.AvailableAmount > 0)
                .ToListAsync();

            if (input.Refunds is null)
            {
                input.Refunds = refundAmounts;
            }
            else
            {
                foreach (var item in refundAmounts)
                {
                    item.RefundAmount = input.Refunds.FirstOrDefault(o =>
                        o.PeriodId == item.PeriodId &&
                        o.CategoryId == item.CategoryId &&
                        o.SubcategoryId == item.SubcategoryId &&
                        o.BucketId == item.BucketId
                    )?.RefundAmount ?? 0;
                }
                input.Refunds = refundAmounts;
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var batch = await db.Batches(id).SingleOrDefaultAsync();

            if (batch == null)
            {
                return NotFound();
            }

            return View(batch);
        }

        [Authorize(Policy.Process)]
        [HttpPost("{id}")]
        public async Task<IActionResult> Details(int id, [FromForm] Batch input)
        {
            var batch = await db.Batches(id).SingleOrDefaultAsync();

            if (batch == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                batch.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Saved changes.");
                    return RedirectToAction("Details", new { id = batch.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(batch);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/post")]
        public async Task<IActionResult> Post(int id, bool skipDepositEqualityCheck = false)
        {
            var batch = await db.Batches(id).SingleOrDefaultAsync();

            if (batch == null)
            {
                return NotFound();
            }

            if (batch.IsPosted)
            {
                ModelState.AddModelError("", "This batch is already posted.");
            }

            if (!skipDepositEqualityCheck && !batch.IsBalanced)
            {
                ModelState.AddModelError("", "A batch can only be posted if the deposit control amount, total deposited, and rev+ovr are all equal.");
                ViewBag.DisplaySkipDepositEqualityCheck = true;
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                batch.Post(skipDepositEqualityCheck);
                db.Comment($"Posted {batch} batch.", new BatchComment(batch));

                try
                {
                    await db.SaveChangesAsync();
                    TempData.Success("Posted batch.");
                    return RedirectToAction("Details", new { id = batch.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(batch);
        }


        [Authorize(Policy.Manage)]
        [Route("{id}/unpost")]
        public async Task<IActionResult> Unpost(int id)
        {
            var batch = await db.Batches(id).SingleOrDefaultAsync();

            if (batch == null)
            {
                return NotFound();
            }

            if (!batch.IsPosted)
            {
                ModelState.AddModelError("", "This batch is already unposted.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                batch.Unpost();
                db.Comment($"Unposted {batch} batch.", new BatchComment(batch));

                try
                {
                    await db.SaveChangesAsync();

                    return RedirectToAction("Details", new { id = batch.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(batch);
        }


        [Authorize(Policy.Process)]
        [Route("generate-dues")]
        public async Task<IActionResult> GenerateDues(
            [FromServices] DueCalculator service,
            GenerateDuesViewModel input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                try
                {
                    var transactions = await service.CalculateDuesAsync(input.EffectiveDate, input.OrganizationIds, Enumerable.Empty<int>());

                    if (!transactions.Any())
                    {
                        ModelState.AddModelError("", "No transactions were found that matched the criteria.");
                    }
                    else
                    {
                        Batch batch = new()
                        {
                            Name = await db.Batches().NextNameAsync("DUE"),
                            Note = $"Automatic calculation of tax, penalty and interest Dues.",
                            Transactions = new(transactions),
                        };
                        db.Add(batch);
                        await db.SaveChangesAsync();

                        TempData.Success("Batch opened.");

                        return RedirectToAction("Details", new { id = batch.Id });
                    }
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
        [Route("manage-money")]
        public async Task<IActionResult> ManageMoney(
            [FromServices] TransferMoneyService service,
            TransferMoneyInput input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                try
                {
                    var batch = await service.ProcessAsync(input);

                    if (batch is null)
                    {
                        ModelState.AddModelError("", "No transactions were found that matched the criteria.");
                    }
                    else
                    {
                        if (input.BatchId is null)
                        {
                            TempData.Success("Batch opened.");
                        }
                        else
                        {
                            TempData.Success("Batch updated.");
                        }

                        return RedirectToAction("Details", new { id = batch.Id });
                    }
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
        [Route("refresh-returns")]
        public async Task<IActionResult> RefreshReturns(
            [FromServices] ReturnRefresher service)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                try
                {
                    var batch = await service.RefreshReturnsAsync();

                    if (batch != null)
                    {
                        TempData.Success("Refreshed returns. A refresh returns batch was created.");
                        return RedirectToAction("Details", new { id = batch.Id });
                    }
                    else
                    {
                        TempData.Success("Refreshed returns. A batch did not need to be created.");
                        return RedirectToAction("RefreshReturns");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View();
        }


        [Authorize(Policy.Manage)]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await db.Batches(id).SingleOrDefaultAsync();

            if (batch == null)
            {
                return NotFound();
            }

            if (!batch.CanModify())
            {
                ModelState.AddModelError("", "The batch cannot be modified.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                var entityType = db.Model.FindEntityType<Batch>();

                try
                {
                    await db.Database.GetDbConnection().ExecuteAsync($"DELETE FROM {entityType.GetSchemaQualifiedTableName()} WHERE \"id\" = @Id", new { Id = id });
                    TempData.Success("Deleted batch.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(batch);
        }
    }
}
