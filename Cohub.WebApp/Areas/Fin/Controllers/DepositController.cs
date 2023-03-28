using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Deposits;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Info;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Fin")]
    [Route("fin/deposit")]
    public class DepositController : Controller
    {
        private readonly ILogger<TransactionsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;
        private readonly DepositService depositService;

        public DepositController(ILogger<TransactionsController> logger, Actor actor, CohubDbContext db, DepositService depositService)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
            this.depositService = depositService;
        }


        [HttpGet]
        public async Task<ActionResult> Index(int batchId = 0, bool isZero = false)
        {
            DepositInfo input = new()
            {
                DefaultDepositDate = DateTime.Today,
            };

            if (batchId > 0)
            {
                var batch = await db.Batches(batchId).SingleOrDefaultAsync();
                if (batch != null && !batch.IsPosted)
                {
                    input.BatchId = batchId;
                }
            }

            ViewBag.IsZero = isZero;
            return View(input);
        }

        [HttpPost]
        public async Task<ActionResult> Index(DepositInfo input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Batch batch;
                    using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        batch = await depositService.ProcessDepositInfoAsync(input);
                        await tx.CommitAsync();
                    }

                    TempData.Success("Deposited payment.");
                    return RedirectToAction("Details", "Batches", new { id = batch.Id });
                }
                catch (MissingFilingsDepositServiceException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.ShowOptionToAllowMissingFilings = true;
                }
                catch (DepositServiceException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            foreach (var deposit in input.Deposits)
            {
                foreach (var payment in deposit.Payments)
                {
                    if (payment.OrganizationId != null && payment.CategoryId != null && payment.PeriodId != null)
                    {
                        ReturnBalance? returnBalance = await db
                            .ReturnBalances(o =>
                                EF.Functions.ILike(o.OrganizationId, payment.OrganizationId) &&
                                EF.Functions.ILike(o.CategoryId, payment.CategoryId) &&
                                EF.Functions.ILike(o.PeriodId, payment.PeriodId) &&
                                ReturnStatus.OpenIds.Contains(o.StatusId)
                            )
                            .SingleOrDefaultAsync();

                        if (returnBalance != null)
                        {
                            // Set return ID
                            payment.ReturnId = returnBalance.Return.Id;

                            // Ensure casing matches
                            payment.OrganizationId = returnBalance.Organization.Id;
                            payment.CategoryId = returnBalance.Category.Id;
                            payment.PeriodId = returnBalance.Period.Id;

                            if (deposit.DepositDate != null)
                            {
                                payment.Snapshot = depositService.CalculatePaymentSnapshot(deposit.DepositDate.Value, returnBalance);
                            }
                        }
                    }

                    // Provide an empty snapshot if we weren't able to make one
                    payment.Snapshot ??= PaymentSnapshot.CreateEmptySnapshot();
                }
            }

            return View(input);
        }

        [HttpPost("refresh-deposit")]
        public async Task<ActionResult> RefreshDeposit(Deposit input, bool isZero = false)
        {
            var organizations = await db.Organizations().Where(o => EF.Functions.ILike(o.Id, input.DepositorId)).Take(2).ToArrayAsync();

            if (organizations.Length == 1)
            {
                var organization = organizations[0];

                var today = DateTime.Today;
                var returnBalances = await db
                    .ReturnBalances(o =>
                        o.OrganizationId == organization.Id &&
                        (o.StatusId == ReturnStatusId.Due || (o.StatusId == ReturnStatusId.Payable && o.Period.EndDate < today))
                    )
                    .ToArrayAsync();

                try
                {
                    var deposit = new Deposit
                    {
                        DepositorId = organization.Id,
                        DepositAmount = input.DepositAmount ?? (isZero ? 0 : null),
                        DepositDate = input.DepositDate ?? returnBalances.Max(o => o.Period.DueDate as DateTime?)
                    };
                    foreach (var returnBalance in returnBalances.OrderBy(o => o.Period.DueDate))
                    {
                        if (isZero)
                        {
                            var zeroPayment = new DepositPayment
                            {
                                ReturnId = returnBalance.Return.Id,
                                OrganizationId = returnBalance.Organization.Id,
                                CategoryId = returnBalance.Category.Id,
                                PeriodId = returnBalance.Period.Id,
                                PaymentAmount = 0,
                                Taxable = 0,
                                Excess = 0,
                            };

                            returnBalance.Return = await depositService.AddOrUpdateReturnAsync(deposit, zeroPayment);
                            returnBalance.LastFiling = returnBalance.Return.GetLatestFiling();
                        }

                        DepositPayment payment = await ActivatePayment(deposit, returnBalance, isZero);
                        deposit.Payments.Add(payment);
                    }

                    return Ok(deposit);
                }
                catch (DepositServiceException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return Ok(input);
            }
        }

        [HttpPost("refresh-payment")]
        public async Task<ActionResult> RefreshPayment([Bind(Prefix = "deposit")] Deposit deposit, [Bind(Prefix = "payment")] DepositPayment payment, bool isZero = false)
        {
            if (payment.OrganizationId.IsNullOrWhiteSpace() ||
                (await db.Organizations().Where(o => EF.Functions.ILike(o.Id, payment.OrganizationId)).Take(2).ToListAsync() is var organizations && organizations.Count != 1))
            {
                return BadRequest("Organization not found.");
            }
            if (payment.CategoryId.IsNullOrWhiteSpace() ||
                (await db.Categories().Where(o => EF.Functions.ILike(o.Id, payment.CategoryId)).Take(2).ToListAsync() is var categories && categories.Count != 1))
            {
                return BadRequest("Category not found.");
            }
            if (payment.PeriodId.IsNullOrWhiteSpace() ||
                (await db.Periods().Where(o => EF.Functions.ILike(o.Id, payment.PeriodId)).Take(2).ToListAsync() is var periods && periods.Count != 1))
            {
                return BadRequest("Period not found.");
            }

            payment.OrganizationId = organizations[0].Id;
            payment.CategoryId = categories[0].Id;
            payment.PeriodId = periods[0].Id;

            try
            {
                var returnBalance = await db.ReturnBalances(new ReturnBalanceFilter
                {
                    OrganizationId = payment.OrganizationId,
                    CategoryId = payment.CategoryId,
                    PeriodId = payment.PeriodId,
                }).SingleOrDefaultAsync();

                if (returnBalance != null)
                {
                    returnBalance.Return = await depositService.AddOrUpdateReturnAsync(deposit, payment);
                    returnBalance.LastFiling = returnBalance.Return.GetLatestFiling();
                }
                else
                {
                    // Create a return balance for a non-existent return.

                    var @return = await depositService.AddOrUpdateReturnAsync(deposit, payment);
                    var paymentConfiguration = (await db
                        .PaymentConfigurations()
                        .SingleOrDefaultAsync(pc =>
                            pc.PaymentChart.CategoryId == @return.CategoryId &&
                            pc.PaymentChart.Frequency.Periods!.Any(p => p.Id == @return.PeriodId && pc.StartDate <= p.DueDate && p.DueDate <= pc.EndDate)
                        )
                    ) ?? new PaymentConfiguration();

                    returnBalance = new ReturnBalance
                    {
                        Return = @return,
                        LastFiling = @return.GetLatestFiling(),

                        Organization = organizations[0],
                        Category = categories[0],
                        Period = periods[0],

                        PaymentConfiguration = paymentConfiguration,
                    };
                }

                DepositPayment model = await ActivatePayment(deposit, returnBalance, isZero);
                model.PaymentAmount = payment.PaymentAmount ?? (isZero && returnBalance.Period.IsPayable() ? 0 : null);

                return Ok(model);
            }
            catch (DepositServiceException ex)
            {
                logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        private async Task<DepositPayment> ActivatePayment(Deposit deposit, ReturnBalance returnBalance, bool isZero)
        {
            var @return = returnBalance.Return;
            if (@return.Filings == null)
            {
                if (@return.Id > 0)
                {
                    await db.Entry(@return).Collection(o => o.Filings).LoadAsync();
                }
                else
                {
                    @return.Filings = new System.Collections.Generic.List<Filing>();
                }
            }

            var snapshot = depositService.CalculatePaymentSnapshot(deposit.DepositDate ?? DateTime.Today, returnBalance);

            var record = new DepositPayment
            {
                OrganizationId = @return.OrganizationId,
                CategoryId = @return.CategoryId,
                PeriodId = @return.PeriodId,
                ReturnId = @return.Id,
                PaymentAmount = (isZero && returnBalance.Period.IsPayable()) ? 0 : null,
                Assessment = (returnBalance.LastFiling as AssessmentFiling)?.AssessmentAmount,
                // Read only info
                Snapshot = snapshot,
            };

            if (returnBalance.Category.TypeId == CategoryTypeId.Fee)
            {
                if (returnBalance.LastFiling is FeeFiling feeFiling)
                {
                    record.Fees = feeFiling.FeeAmount;
                }
                else if (isZero && returnBalance.Period.IsPayable())
                {
                    record.Fees = 0;
                }
            }

            if (returnBalance.Category.TypeId == CategoryTypeId.Tax)
            {
                if (returnBalance.LastFiling is TaxFiling taxFiling)
                {
                    record.Taxable = taxFiling.TaxableAmount;
                    record.Excess = taxFiling.ExcessTax;
                }
                else if (isZero && returnBalance.Period.IsPayable())
                {
                    record.Taxable = 0;
                    record.Excess = 0;
                }
            }

            return record;
        }
    }
}
