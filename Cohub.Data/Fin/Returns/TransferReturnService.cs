using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SiteKit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Returns
{
    public class TransferReturnService
    {
        private readonly IServiceProvider services;
        private readonly ILogger<TransferReturnService> logger;

        public TransferReturnService(IServiceProvider services, ILogger<TransferReturnService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public async Task<Batch?> ProcessAsync(TransferReturn input)
        {
            // Use a scoped DbContext
            using var scope = services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<CohubDbContext>();

            var @return = await db.Returns().SingleOrDefaultAsync(o => o.Id == input.ReturnId);

            if (@return is null)
            {
                throw new ValidationException($"The source return was not found.");
            }

            if (@return.StatusId == ReturnStatusId.Closed)
            {
                throw new ValidationException($"The {@return} return is closed and cannot be modified.");
            }

            if (@return.OrganizationId == input.DestinationOrganizationId &&
                @return.PeriodId == input.DestinationPeriodId &&
                @return.CategoryId == input.DestinationCategoryId)
            {
                throw new ValidationException($"The source return and destination are the same.");
            }

            if (@return.CategoryId != input.DestinationCategoryId)
            {
                throw new ValidationException($"The source and destination categories are different. This process cannot be used to transfer a return to a new category.");
            }

            {
                var destinationReturn = await db.Returns()
                    .Where(o =>
                        o.OrganizationId == input.DestinationOrganizationId &&
                        o.PeriodId == input.DestinationPeriodId &&
                        o.CategoryId == input.DestinationCategoryId
                    )
                    .Select(r => new
                    {
                        Return = r,
                        HasFilings = r.Filings.Any(),
                        RevOvrBalance = db.TransactionDetails()
                            .Where(td =>
                                new[] { BucketId.Revenue, BucketId.Overpayment }.Contains(td.BucketId) &&
                                td.OrganizationId == r.OrganizationId &&
                                td.PeriodId == r.PeriodId &&
                                td.CategoryId == r.CategoryId
                            )
                            .Sum(td => td.Amount)
                    })
                    .SingleOrDefaultAsync();
                if (destinationReturn is not null)
                {
                    if (destinationReturn.HasFilings && destinationReturn.RevOvrBalance != 0)
                    {
                        throw new ValidationException($"A return already exists for {destinationReturn} that has filings and revenue/overpayment balance. That return must be deleted before the {@return} return can be transfered to {destinationReturn}.");
                    }
                    else if (destinationReturn.HasFilings)
                    {
                        throw new ValidationException($"A return already exists for {destinationReturn} that has filings. That return must be deleted before the {@return} return can be transfered to {destinationReturn}.");
                    }
                    else if (destinationReturn.RevOvrBalance != 0)
                    {
                        throw new ValidationException($"A return already exists for {destinationReturn} that has revenue/overpayment balance. That return must be deleted before the {@return} return can be transfered to {destinationReturn}.");
                    }
                    else
                    {
                        // It is safe to delete the destination return
                        db.Comment($"Deleted {destinationReturn.Return} return.", new ReturnComment(destinationReturn.Return.Id), new OrganizationComment(destinationReturn.Return.OrganizationId));
                        db.Remove(destinationReturn.Return);
                    }
                }
            }

            var destinationName = $"{input.DestinationOrganizationId}: {input.DestinationPeriodId} {input.DestinationCategoryId}";

            var comment = db.Comment($"Transfered {@return} return to {destinationName}.", new ReturnComment(@return.Id), new OrganizationComment(@return.OrganizationId));
            if (@return.OrganizationId != input.DestinationOrganizationId)
            {
                db.ReferenceComment(comment, new OrganizationComment(input.DestinationOrganizationId));
            }

            // Transfer balances.
            Batch? batch;
            List<Transaction> transactions = new();

            var destinationDueTransactionDetails = await db.TransactionDetails()
                .Where(td =>
                    td.BucketId == BucketId.Due &&
                    td.OrganizationId == input.DestinationOrganizationId &&
                    td.CategoryId == input.DestinationCategoryId &&
                    td.PeriodId == input.DestinationPeriodId
                )
                .ToListAsync();
            if (!destinationDueTransactionDetails.Sum(o => o.Amount).IsZeroCents())
            {
                var transaction = new Transaction
                {
                    Note = $"Zero {destinationName} dues as part of transferring {@return} return to {destinationName}.",
                    Details = new(),
                };
                transactions.Add(transaction);
                foreach (var detail in destinationDueTransactionDetails)
                {
                    var zeroDue = new TransactionDetail
                    {
                        OrganizationId = detail.OrganizationId,
                        CategoryId = detail.CategoryId,
                        PeriodId = detail.PeriodId,

                        BucketId = detail.BucketId,
                        SubcategoryId = detail.SubcategoryId,
                        EffectiveDate = detail.EffectiveDate,
                        Amount = -detail.Amount,
                        Note = detail.Note,
                    };
                    transaction.Details.Add(zeroDue);
                }
            }

            var transactionDetails = await db.TransactionDetails()
                .Where(td =>
                    td.OrganizationId == @return.OrganizationId &&
                    td.CategoryId == @return.CategoryId &&
                    td.PeriodId == @return.PeriodId
                )
                .OrderBy(o => o.Created).ThenBy(o => o.EffectiveDate).ThenBy(o => o.Id)
                .ToListAsync();

            if (transactionDetails.Any())
            {
                var transaction = new Transaction
                {
                    Note = $"Transfer balances from {@return} to {destinationName}.",
                    Details = new(),
                };
                transactions.Add(transaction);
                foreach (var detail in transactionDetails)
                {
                    var reversal = new TransactionDetail
                    {
                        OrganizationId = detail.OrganizationId,
                        CategoryId = detail.CategoryId,
                        PeriodId = detail.PeriodId,

                        BucketId = detail.BucketId,
                        SubcategoryId = detail.SubcategoryId,
                        EffectiveDate = detail.EffectiveDate,
                        Amount = -detail.Amount,
                        Note = detail.Note,
                    };
                    var copy = new TransactionDetail
                    {
                        OrganizationId = input.DestinationOrganizationId,
                        CategoryId = input.DestinationCategoryId,
                        PeriodId = input.DestinationPeriodId,

                        BucketId = detail.BucketId,
                        SubcategoryId = detail.SubcategoryId,
                        EffectiveDate = detail.EffectiveDate,
                        Amount = detail.Amount,
                        Note = detail.Note,
                    };
                    transaction.Details.Add(reversal);
                    transaction.Details.Add(copy);
                }
            }

            if (transactions.Any())
            {
                batch = new Batch
                {
                    Name = await db.Batches().NextNameAsync("XFER-RETURN"),
                    DepositControlAmount = transactions.Sum(t => t.Deposited) ?? 0,
                    Transactions = transactions,
                    Note = $"Transfer balances from {@return} to {destinationName}.",
                };
                batch.Post(skipDepositEqualityCheck: true);
                db.Add(batch);
                db.ReferenceComment(comment, new BatchComment(batch));
            }
            else
            {
                batch = null;
            }

            var originalOrganizationId = @return.OrganizationId;
            var originalPeriodId = @return.PeriodId;

            // Transfer the return to the destination.
            @return.OrganizationId = input.DestinationOrganizationId;
            @return.PeriodId = input.DestinationPeriodId;
            @return.CategoryId = input.DestinationCategoryId;

            await db.SaveChangesAsync();

            try
            {
                var returnGenerator = scope.ServiceProvider.GetRequiredService<ReturnGenerator>();
                var returnRefresher = scope.ServiceProvider.GetRequiredService<ReturnRefresher>();

                // Ensure that if a return should exist it will exist.
                var originalPeriod = await db.Periods().FindAsync(originalPeriodId);
                await returnGenerator.GenerateMissingReturnsAsync(originalPeriod.StartDate, originalPeriod.EndDate, new[] { originalOrganizationId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
            }

            return batch;
        }
    }
}
