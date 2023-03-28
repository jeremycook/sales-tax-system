using Cohub.Data.Fin.Batches;
using Cohub.Data.Fin.Deposits;
using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using SiteKit.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Returns
{
    /// <summary>
    /// A <see cref="Return"/> with the <see cref="ReturnStatusId.Payable"/> status
    /// will be changed to <see cref="ReturnStatusId.Due"/> once its due date passes.
    /// A <see cref="Return"/> will be changed to <see cref="ReturnStatusId.Closed"/> 
    /// if it is paid in full, or after three years.
    /// </summary>
    public class ReturnRefresher
    {
        private readonly CohubDbContext db;
        private readonly DepositService depositService;
        private readonly TransferMoneyService transferMoneyService;

        public ReturnRefresher(CohubDbContext db, DepositService depositService, TransferMoneyService transferMoneyService)
        {
            this.db = db;
            this.depositService = depositService;
            this.transferMoneyService = transferMoneyService;
        }

        public async Task<Batch?> RefreshReturnsAsync()
        {
            if (db.ChangeTracker.HasChanges())
            {
                throw new InvalidOperationException("The db has changes. Save changes before calling this method.");
            }

            using var tx = await db.Database.BeginTransactionAsync();

            var zeroOutDuesBatch = new Batch
            {
                Name = await db.Batches().NextNameAsync("REFRESH-RETURNS"),
                Note = $"Zero out dues of returns closed by the return refresher.",
                Transactions = new(),
            };
            db.Add(zeroOutDuesBatch);
            await db.SaveChangesAsync();

            // Change payable returns is to due once their due date has past.
            var nowDueReturns = await db.Returns()
                .Where(r => r.StatusId == ReturnStatusId.Payable && r.Period.DueDate < DateTime.Today)
                .ToListAsync();
            foreach (var ret in nowDueReturns)
            {
                ret.StatusId = ReturnStatusId.Due;
            }
            await db.SaveChangesAsync();

            // Close the return if it was due 3 or more years ago
            var expiredReturns = await db.Returns()
                .Where(r => r.StatusId == ReturnStatusId.Due && r.Period.DueDate < DateTime.Today.AddYears(-3))
                .ToListAsync();
            foreach (var ret in expiredReturns)
            {
                bool zeroedOutDues = null != await transferMoneyService.ProcessAsync(new TransferMoneyInput
                {
                    BatchId = zeroOutDuesBatch.Id,
                    Action = TransferMoneyAction.AdjustDues,
                    OrganizationId = ret.OrganizationId,
                    CategoryId = ret.CategoryId,
                    PeriodId = ret.PeriodId,
                });

                ret.StatusId = ReturnStatusId.Closed;
                db.Comment($"Closed 3 year old return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId), new BatchComment(zeroOutDuesBatch));
            }
            await db.SaveChangesAsync();

            // Close the return if net, penalty and interest dues have been paid,
            // the balance is zero, nothing due and no credits
            var openFiledReturns = await db
                .ReturnBalances(r =>
                    r.StatusId != ReturnStatusId.Closed &&
                    r.Filings.Any()
                )
                .ToListAsync();
            foreach (var returnBalance in openFiledReturns)
            {
                var ret = returnBalance.Return;

                var snapshot = depositService.CalculatePaymentSnapshot(returnBalance.MostRecentDateRevOrAdjEffectiveDate ?? DateTime.Today, returnBalance);

                decimal totalPaid = (returnBalance.Overpayment + returnBalance.TotalNetRevAndAdj + returnBalance.TotalPenaltyRevAndAdj + returnBalance.TotalInterestRevAndAdj).RoundAwayFromZero();
                decimal totalDue = (snapshot.NetAmount + snapshot.PenaltyDue + snapshot.InterestDue).RoundAwayFromZero();
                if (totalPaid >= totalDue)
                {
                    bool zeroedOutDues = null != await transferMoneyService.ProcessAsync(new TransferMoneyInput
                    {
                        BatchId = zeroOutDuesBatch.Id,
                        Action = TransferMoneyAction.EraseDues,
                        OrganizationId = ret.OrganizationId,
                        CategoryId = ret.CategoryId,
                        PeriodId = ret.PeriodId,
                    });

                    ret.StatusId = ReturnStatusId.Closed;

                    db.Comment($"Closed paid off return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId), new BatchComment(zeroOutDuesBatch));
                }
            }
            await db.SaveChangesAsync();

            await db.Entry(zeroOutDuesBatch).Collection(o => o.Transactions).LoadAsync();
            if (zeroOutDuesBatch.Transactions.Any())
            {
                if (zeroOutDuesBatch.CanPost())
                {
                    zeroOutDuesBatch.Post(skipDepositEqualityCheck: true);
                    await db.SaveChangesAsync();
                }
            }
            else
            {
                db.Remove(zeroOutDuesBatch);
                await db.SaveChangesAsync();
            }

            await tx.CommitAsync();

            return zeroOutDuesBatch.Transactions.Any() ?
                zeroOutDuesBatch :
                null;
        }
    }
}
