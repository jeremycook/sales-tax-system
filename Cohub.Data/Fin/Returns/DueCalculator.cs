using Cohub.Data.Fin.Deposits;
using Microsoft.EntityFrameworkCore;
using SiteKit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Returns
{
    /// <summary>
    /// Fills a <see cref="Batch"/> of
    /// <see cref="BucketId.Due"/> <see cref="TransactionDetail"/>s
    /// for <see cref="ReturnStatusId.Due"/> <see cref="Return"/>s.
    /// </summary>
    public class DueCalculator
    {
        private readonly CohubDbContext db;
        private readonly DepositService depositService;

        public DueCalculator(CohubDbContext db, DepositService depositService)
        {
            this.db = db;
            this.depositService = depositService;
        }

        /// <summary>
        /// Balance tax, penalty and interest Due through <paramref name="effectiveDate"/>.
        /// </summary>
        /// <param name="effectiveDate"></param>
        /// <param name="organizationIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Transaction>> CalculateDuesAsync(DateTime effectiveDate, IEnumerable<string> organizationIds, IEnumerable<int> returnIds)
        {
            if (db.ChangeTracker.HasChanges())
            {
                throw new InvalidOperationException("The db has changes. Save changes before calling this method.");
            }

            var transactions = new List<Transaction>();

            var dueReturns = await db
                .ReturnBalances(r =>
                    r.StatusId == ReturnStatusId.Due &&
                    (!organizationIds.Any() || organizationIds.Contains(r.OrganizationId)) &&
                    (!returnIds.Any() || returnIds.Contains(r.Id))
                )
                .ToArrayAsync();

            foreach (var returnBalance in dueReturns)
            {
                var transaction = new Transaction
                {
                    Details = new List<TransactionDetail>()
                };

                var toDateSnapshot = depositService.CalculatePaymentSnapshot(effectiveDate, returnBalance);

                var unpaidTax = toDateSnapshot.NetAmount.RoundAwayFromZero() - returnBalance.TotalNetRevAndAdj;
                var unaccountedTax = unpaidTax - returnBalance.NetDue.RoundAwayFromZero();
                if (unaccountedTax != 0)
                {
                    transaction.Details.Add(TransactionDetail.NetDue(returnBalance.Return.CategoryId, returnBalance.Return.OrganizationId, returnBalance.Return.PeriodId, toDateSnapshot.PaymentDate, unaccountedTax));
                }

                var unpaidInterest = toDateSnapshot.InterestDue.RoundAwayFromZero() - returnBalance.TotalInterestRevAndAdj;
                var unaccountedInterest = unpaidInterest - returnBalance.InterestDue.RoundAwayFromZero();
                if (unaccountedInterest != 0)
                {
                    transaction.Details.Add(TransactionDetail.InterestDue(returnBalance.Return.CategoryId, returnBalance.Return.OrganizationId, returnBalance.Return.PeriodId, toDateSnapshot.PaymentDate, unaccountedInterest));
                }

                var unpaidPenalty = toDateSnapshot.PenaltyDue.RoundAwayFromZero() - returnBalance.TotalPenaltyRevAndAdj;
                var unaccountedPenalty = unpaidPenalty - returnBalance.PenaltyDue.RoundAwayFromZero();
                if (unaccountedPenalty != 0)
                {
                    transaction.Details.Add(TransactionDetail.PenaltyDue(returnBalance.Return.CategoryId, returnBalance.Return.OrganizationId, returnBalance.Return.PeriodId, toDateSnapshot.PaymentDate, unaccountedPenalty));
                }

                if (transaction.Details.Any())
                {
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        /// <summary>
        /// Generates an open batch that balances tax, penalty and interest Due through today.
        /// If no changes are made a batch will not be generated.
        /// </summary>
        /// <returns></returns>
        public async Task<Batch?> GenerateDueBatchAsync(DateTime effectiveDate, IEnumerable<string> organizationIds, IEnumerable<int> returnIds)
        {
            var transactions = await CalculateDuesAsync(
                effectiveDate: effectiveDate,
                organizationIds: organizationIds,
                returnIds: returnIds);

            if (!transactions.Any())
            {
                return null;
            }

            Batch batch = new()
            {
                Name = await db.Batches().NextNameAsync("DUE"),
                Note = $"Automatic calculation of tax, penalty and interest Dues.",
                Transactions = new(transactions),
            };
            db.Add(batch);

            await db.SaveChangesAsync();

            return batch;
        }

        /// <summary>
        /// Generates an open batch that balances tax, penalty and interest Due through today.
        /// If no changes are made a batch will not be generated.
        /// </summary>
        /// <returns></returns>
        public async Task<Batch?> GenerateDueBatchAsync()
        {
            return await GenerateDueBatchAsync(DateTime.Today, Array.Empty<string>(), Array.Empty<int>());
        }
    }
}