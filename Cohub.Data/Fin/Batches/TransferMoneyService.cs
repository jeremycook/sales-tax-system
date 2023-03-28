using Microsoft.EntityFrameworkCore;
using SiteKit.Extensions;
using SiteKit.Info;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Batches
{
    public class TransferMoneyService
    {
        private readonly CohubDbContext db;
        private readonly Actor actor;

        public TransferMoneyService(CohubDbContext db, Actor actor)
        {
            this.db = db;
            this.actor = actor;
        }

        /// <summary>
        /// Returns a batch if any changes are made and saved.
        /// Returns null if no changes are made.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<Batch?> ProcessAsync(TransferMoneyInput input)
        {
            List<Transaction> transactions;
            if (input.Action == TransferMoneyAction.ApplyOverpayments)
            {
                // Applying overpayments to dues is special.
                transactions = await ApplyOverpaymentsToDuesAsync(input);
            }
            else
            {
                string bucketId;
                string? destinationBucketId;
                switch (input.Action)
                {
                    case TransferMoneyAction.AdjustDues:
                        bucketId = BucketId.Due;
                        destinationBucketId = BucketId.Adjustment;
                        break;
                    case TransferMoneyAction.ClearOverpayments:
                        bucketId = BucketId.Overpayment;
                        destinationBucketId = BucketId.Revenue;
                        break;
                    case TransferMoneyAction.EraseDues:
                        bucketId = BucketId.Due;
                        destinationBucketId = null;
                        break;
                    default:
                        throw new NotSupportedException($"The {input.Action} transfer money action is not supported.");
                }


                var minMaxGroups = (
                    from o in db.TransactionDetails()
                    group o by new
                    {
                        o.BucketId,
                        o.OrganizationId,
                        o.PeriodId,
                        o.CategoryId,
                    } into g
                    where
                        g.Sum(x => x.Amount) != 0 &&
                        (input.MinimumAmount == null || input.MinimumAmount <= g.Sum(x => x.Amount)) &&
                        (input.MaximumAmount == null || g.Sum(x => x.Amount) <= input.MaximumAmount)
                    select g.Key
                );

                var candidates = await db.TransactionDetails()
                    // Filter based on min/max group first.
                    .Where(o => minMaxGroups.Any(g => g.OrganizationId == o.OrganizationId && g.PeriodId == o.PeriodId && g.CategoryId == o.CategoryId && g.BucketId == o.BucketId))
                    .Where(o => input.OrganizationId == null || o.OrganizationId == input.OrganizationId)
                    .Where(o => input.CategoryId == null || o.CategoryId == input.CategoryId)
                    .Where(o => input.PeriodId == null || o.PeriodId == input.PeriodId)
                    .Where(o => input.SubcategoryId == null || o.SubcategoryId == input.SubcategoryId)
                    .Where(o => o.BucketId == bucketId)
                    // Apply the return status filter last since it is the most complex
                    .Where(o => input.ReturnStatusId == null || db.Returns().Any(r =>
                            r.OrganizationId == o.OrganizationId &&
                            r.CategoryId == o.CategoryId &&
                            r.PeriodId == o.PeriodId &&
                            r.StatusId == input.ReturnStatusId.Value
                        )
                    )
                    .Where(o => input.HasFiled == null || input.HasFiled == db.Filings().Any(r =>
                            r.Return.OrganizationId == o.OrganizationId &&
                            r.Return.CategoryId == o.CategoryId &&
                            r.Return.PeriodId == o.PeriodId
                        )
                    )
                    .GroupBy(o => new
                    {
                        o.BucketId,
                        o.OrganizationId,
                        o.PeriodId,
                        o.CategoryId,
                        o.SubcategoryId,
                        o.EffectiveDate,
                    })
                    .Select(o => new Record
                    {
                        BucketId = o.Key.BucketId,
                        OrganizationId = o.Key.OrganizationId,
                        PeriodId = o.Key.PeriodId,
                        CategoryId = o.Key.CategoryId,
                        SubcategoryId = o.Key.SubcategoryId,
                        EffectiveDate = o.Key.EffectiveDate,
                        Total = o.Sum(x => x.Amount),
                    })
                    .ToListAsync();

                transactions = new List<Transaction>();

                foreach (var orgGroup in candidates.GroupBy(o => o.OrganizationId))
                {
                    var transaction = new Transaction
                    {
                        Note = $"Reduce {bucketId} to zero and increase {destinationBucketId ?? "no bucket"} by the same amount with an amount between {input.MinimumAmount?.ToString() ?? "any"} and {input.MaximumAmount?.ToString() ?? "any"} for {input.OrganizationId ?? "any"} organization, {input.CategoryId ?? "any"} category, {input.PeriodId ?? "any"} period, {input.SubcategoryId ?? "any"} subcategory, and {input.ReturnStatusId?.ToString() ?? "any"} return status.",
                        Details = new(),
                    };

                    foreach (var record in orgGroup)
                    {
                        transaction.Details.Add(new()
                        {
                            BucketId = record.BucketId,
                            OrganizationId = record.OrganizationId,
                            PeriodId = record.PeriodId,
                            CategoryId = record.CategoryId,
                            SubcategoryId = record.SubcategoryId,
                            Amount = -record.Total,
                            EffectiveDate = input.NewEffectiveDate ?? record.EffectiveDate,
                        });

                        if (destinationBucketId is not null)
                        {
                            transaction.Details.Add(new()
                            {
                                BucketId = destinationBucketId,
                                OrganizationId = record.OrganizationId,
                                PeriodId = record.PeriodId,
                                CategoryId = record.CategoryId,
                                SubcategoryId = record.SubcategoryId,
                                Amount = record.Total,
                                EffectiveDate = input.NewEffectiveDate ?? record.EffectiveDate,
                            });
                        }

                        record.Total -= record.Total;
                    }

                    transactions.Add(transaction);
                }
            }

            if (transactions.Any())
            {
                Batch batch;
                if (input.BatchId is null)
                {
                    batch = new()
                    {
                        Name = await db.Batches().NextNameAsync(actor.Initials ?? actor.Name),
                        Transactions = new(),
                        Note = input.Action.GetDisplayName(),
                    };
                    db.Add(batch);
                }
                else
                {
                    batch = await db.Batches().FindAsync(input.BatchId.Value);

                    if (batch is null)
                    {
                        throw new ValidationException("The batch was not found.");
                    }
                    else if (!batch.CanModify())
                    {
                        throw new ValidationException("The batch cannot be modified.");
                    }

                    if (batch.Transactions == null)
                    {
                        await db.Entry(batch).Collection(o => o.Transactions).LoadAsync();
                        foreach (var transaction in batch.Transactions!)
                        {
                            await db.Entry(transaction).Collection(o => o.Details).LoadAsync();
                        }
                    }
                }

                batch.Transactions.AddRange(transactions);

                await db.SaveChangesAsync();

                return batch;
            }
            else
            {
                return null;
            }
        }

        private async Task<List<Transaction>> ApplyOverpaymentsToDuesAsync(TransferMoneyInput input)
        {
            var dues = await db.TransactionDetails()
                .Where(o => o.BucketId == BucketId.Due)
                .Where(o => input.OrganizationId == null || o.OrganizationId == input.OrganizationId)
                .Where(o => input.CategoryId == null || o.CategoryId == input.CategoryId)
                .Where(o => input.PeriodId == null || o.PeriodId == input.PeriodId)
                .Where(o => input.SubcategoryId == null || o.SubcategoryId == input.SubcategoryId)
                .Where(o => db.Returns().Any(r =>
                        r.OrganizationId == o.OrganizationId &&
                        r.CategoryId == o.CategoryId &&
                        r.PeriodId == o.PeriodId &&
                        (input.ReturnStatusId == null || r.StatusId == input.ReturnStatusId.Value)
                    )
                )
                // Consider filed returns by default.
                .Where(o => (input.HasFiled ?? true) == db.Filings().Any(r =>
                        r.Return.OrganizationId == o.OrganizationId &&
                        r.Return.CategoryId == o.CategoryId &&
                        r.Return.PeriodId == o.PeriodId
                    )
                )
                .GroupBy(o => new
                {
                    o.BucketId,
                    o.OrganizationId,
                    o.PeriodId,
                    o.CategoryId,
                    o.SubcategoryId,
                })
                .Select(o => new Record
                {
                    BucketId = o.Key.BucketId,
                    OrganizationId = o.Key.OrganizationId,
                    PeriodId = o.Key.PeriodId,
                    CategoryId = o.Key.CategoryId,
                    SubcategoryId = o.Key.SubcategoryId,
                    Total = o.Sum(x => x.Amount),
                })
                .Where(o => o.Total > 0)
                .ToListAsync();

            if (input.MinimumAmount != null || input.MaximumAmount != null)
            {
                var disqualified = new List<Record>();

                // Disqualify candidates with a return total that is outside the amount range.
                foreach (var group in dues.GroupBy(o => new
                {
                    o.BucketId,
                    o.OrganizationId,
                    o.PeriodId,
                    o.CategoryId,
                }))
                {
                    var returnTotal = group.Sum(g => g.Total);

                    if (input.MinimumAmount != null && returnTotal < input.MinimumAmount)
                    {
                        disqualified.AddRange(group);
                    }
                    else if (input.MaximumAmount != null && returnTotal > input.MaximumAmount)
                    {
                        disqualified.AddRange(group);
                    }
                }

                disqualified.ForEach(r => dues.Remove(r));
            }

            if (!dues.Any())
            {
                throw new ValidationException($"No posted, due balances were found that were greater than {0:C2}.");
            }

            var candidateOrganizationIds = dues.Select(o => o.OrganizationId);

            var overpayments = await db.TransactionDetails()
                .Where(o => o.BucketId == BucketId.Overpayment)
                .Where(o => candidateOrganizationIds.Contains(o.OrganizationId))
                .GroupBy(o => new
                {
                    o.BucketId,
                    o.OrganizationId,
                    o.PeriodId,
                    o.CategoryId,
                    o.SubcategoryId,
                })
                .Select(o => new Record
                {
                    BucketId = o.Key.BucketId,
                    OrganizationId = o.Key.OrganizationId,
                    PeriodId = o.Key.PeriodId,
                    CategoryId = o.Key.CategoryId,
                    SubcategoryId = o.Key.SubcategoryId,
                    Total = o.Sum(x => x.Amount),
                })
                .Where(o => o.Total > 0)
                .ToArrayAsync();

            if (!overpayments.Any())
            {
                throw new ValidationException($"No posted, overpayment balances were found.");
            }

            List<Transaction> transactions = new();

            foreach (var overGroup in overpayments.GroupBy(o => new { o.OrganizationId, o.CategoryId, o.PeriodId }))
            {
                var transaction = new Transaction
                {
                    Note = $"Reduce {BucketId.Due} with {BucketId.Overpayment} balances for {overGroup.Key.OrganizationId}: {overGroup.Key.PeriodId} {overGroup.Key.CategoryId}.",
                    Details = new(),
                };

                foreach (var over in overGroup.OrderBy(o => o.Total).ToArray())
                {
                    if (over.Total <= 0)
                    {
                        continue;
                    }

                    // Match due transactions that most closely relate to the overpayment,
                    // falling back to a broader and broader criteria as needed.
                    var matches = dues
                        .Where(o => o.OrganizationId == over.OrganizationId && o.Total > 0)
                        .OrderBy(o =>
                            o.CategoryId == over.CategoryId && o.PeriodId == over.PeriodId ? 0 :
                            o.CategoryId == over.CategoryId ? 1 :
                            o.PeriodId == over.PeriodId ? 2 :
                            3 // OrganizationIds match
                        )
                        .ThenBy(o => o.Total)
                        .ToArray();

                    foreach (var due in matches)
                    {
                        if (due.Total <= 0)
                        {
                            continue;
                        }

                        var offset = -Math.Min(over.Total, due.Total).RoundAwayFromZero();

                        if (offset >= 0)
                        {
                            continue;
                        }

                        transaction.Details.Add(new()
                        {
                            BucketId = BucketId.Due,
                            OrganizationId = due.OrganizationId,
                            PeriodId = due.PeriodId,
                            CategoryId = due.CategoryId,
                            SubcategoryId = due.SubcategoryId,
                            Amount = offset,
                            EffectiveDate = input.NewEffectiveDate ?? DateTime.Today,
                        });
                        transaction.Details.Add(new()
                        {
                            BucketId = BucketId.Revenue,
                            OrganizationId = due.OrganizationId,
                            PeriodId = due.PeriodId,
                            CategoryId = due.CategoryId,
                            SubcategoryId = due.SubcategoryId,
                            Amount = -offset,
                            EffectiveDate = input.NewEffectiveDate ?? DateTime.Today,
                        });
                        transaction.Details.Add(new()
                        {
                            BucketId = BucketId.Overpayment,
                            OrganizationId = over.OrganizationId,
                            PeriodId = over.PeriodId,
                            CategoryId = over.CategoryId,
                            SubcategoryId = over.SubcategoryId,
                            Amount = offset,
                            EffectiveDate = input.NewEffectiveDate ?? DateTime.Today,
                        });

                        over.Total += offset;
                        due.Total += offset;
                    }
                }

                if (transaction.Details.Any())
                {
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        private class Record
        {
            public string BucketId { get; set; } = null!;
            public string OrganizationId { get; set; } = null!;
            public string PeriodId { get; set; } = null!;
            public string CategoryId { get; set; } = null!;
            public string SubcategoryId { get; set; } = null!;
            public DateTime EffectiveDate { get; set; }
            public decimal Total { get; set; }
        }
    }
}
