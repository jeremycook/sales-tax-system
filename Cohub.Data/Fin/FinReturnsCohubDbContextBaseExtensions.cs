using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cohub.Data.Fin
{
    public static class FinReturnsCohubDbContextBaseExtensions
    {

        public static IQueryable<ReturnBalance> ReturnBalances(this CohubDbContextBase db, Expression<Func<Return, bool>> returnPredicate)
        {
            return db.ReturnBalances(db.Set<Return>().Where(returnPredicate));
        }

        public static IQueryable<ReturnBalance> ReturnBalances(this CohubDbContextBase db, ReturnBalanceFilter filter)
        {
            return db.ReturnBalances(o =>
                (filter.OrganizationId == null || o.OrganizationId == filter.OrganizationId) &&
                (filter.CategoryId == null || o.CategoryId == filter.CategoryId) &&
                (filter.PeriodId == null || o.PeriodId == filter.PeriodId)
            );
        }

        public static IQueryable<ReturnBalance> ReturnBalances(this CohubDbContextBase db, IQueryable<Return> returnQuery)
        {
            return returnQuery
                .OrderBy(o => o.OrganizationId)
                .ThenBy(o => o.Period.DueDate)
                .ThenBy(o => o.Period.StartDate)
                .ThenBy(o => o.CategoryId)
                .Select(r => new ReturnBalance
                {
                    Return = r,
                    Organization = r.Organization,
                    Category = r.Category,
                    Period = r.Period,
                    LastFiling = r.Filings.OrderBy(o => o.Created).ThenBy(o => o.Id).LastOrDefault(),
                    PaymentConfiguration = db.Set<PaymentConfiguration>().SingleOrDefault(pc =>
                        pc.PaymentChart.CategoryId == r.CategoryId &&
                        pc.PaymentChart.Frequency.Periods.Any(p => p.Id == r.PeriodId && pc.StartDate <= p.DueDate && p.DueDate <= pc.EndDate)
                    ) ?? new PaymentConfiguration(),
                    MostRecentDateRevOrAdjEffectiveDate = db.Set<TransactionDetail>()
                        .Where(td =>
                            (td.BucketId == BucketId.Revenue || td.BucketId == BucketId.Adjustment) &&
                            td.CategoryId == r.CategoryId &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Select(td => td.EffectiveDate as DateTime?)
                        .Max(),
                    HistoricNetAmountDue = db.Set<TransactionDetail>()
                        .Where(o =>
                            o.BucketId == BucketId.Revenue &&
                            o.SubcategoryId == SubcategoryId.Net &&
                            o.OrganizationId == r.OrganizationId &&
                            o.CategoryId == r.CategoryId &&
                            !db.Set<TransactionDetail>().Any(p => p.OrganizationId == o.OrganizationId && p.CategoryId == o.CategoryId && p.PeriodId == o.PeriodId && p.BucketId == BucketId.Due)
                        )
                        .GroupBy(o => new { o.Period.Id, o.Period.DueDate })
                        .OrderByDescending(o => o.Key.DueDate)
                        .Take(1)
                        .Average(o => o.Sum(p => p.Amount)),
                    Overpayment = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Overpayment &&
                            td.CategoryId == r.CategoryId &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    OnTimeNetRevAndAdj = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.SubcategoryId == SubcategoryId.Net &&
                            (td.BucketId == BucketId.Revenue || td.BucketId == BucketId.Adjustment) &&
                            td.CategoryId == r.CategoryId &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId &&
                            td.EffectiveDate <= r.Period.DueDate
                        )
                        .Sum(td => td.Amount),
                    NetRevenue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Revenue &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Net &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    NetDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Net &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    NetAdjustment = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Adjustment &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Net &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    PenaltyRevenue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Revenue &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Penalty &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    PenaltyDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Penalty &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    PenaltyAdjustment = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Adjustment &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Penalty &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    InterestRevenue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Revenue &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Interest &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    InterestDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Interest &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    InterestAdjustment = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Adjustment &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Interest &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                });
        }

        public static IQueryable<ReturnSummary> ReturnSummaries(this CohubDbContextBase db, Expression<Func<Return, bool>> returnFilter)
        {
            return db.ReturnSummaries(db.Set<Return>().Where(returnFilter));
        }

        public static IQueryable<ReturnSummary> ReturnSummaries(this CohubDbContextBase db, IQueryable<Return> returnQuery)
        {
            return returnQuery
                .OrderBy(o => o.OrganizationId)
                .ThenBy(o => o.Period.DueDate)
                .ThenBy(o => o.Period.StartDate)
                .ThenBy(o => o.CategoryId)
                .Select(r => new ReturnSummary
                {
                    Return = r,
                    Status = r.Status,
                    Organization = r.Organization,
                    Category = r.Category,
                    Period = r.Period,
                    NetRevenue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Revenue &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Net &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    NetDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Net &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    PenaltyDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Penalty &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    InterestDue = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Due &&
                            td.CategoryId == r.CategoryId &&
                            td.SubcategoryId == SubcategoryId.Interest &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    NSFFee = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.CategoryId == CategoryId.NSFFee &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                    Ovr = db.Set<TransactionDetail>()
                        .Where(td =>
                            td.BucketId == BucketId.Overpayment &&
                            td.CategoryId == r.CategoryId &&
                            td.OrganizationId == r.OrganizationId &&
                            td.PeriodId == r.PeriodId
                        )
                        .Sum(td => td.Amount),
                });
        }
    }
}
