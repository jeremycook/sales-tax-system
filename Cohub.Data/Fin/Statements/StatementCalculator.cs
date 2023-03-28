using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Statements
{
    /// <summary>
    /// Generate <see cref="Statement"/>s based <see cref="TransactionDetail"/>s.
    /// </summary>
    public class StatementCalculator
    {
        private static readonly string[] BucketIds = new[] { BucketId.Due, BucketId.Overpayment };
        private readonly CohubDbContext db;

        public StatementCalculator(CohubDbContext db)
        {
            this.db = db;
        }

        public async Task<List<StatementReport>> CalculateStatementReportsAsync(DateTime noticeDate)
        {
            var threeYearsBeforeDateOfNotice = noticeDate.AddYears(-3);
            var data = await db
                .TransactionDetails()
                .Where(td => BucketIds.Contains(td.BucketId))
                .OrderBy(o => o.OrganizationId)
                .GroupBy(o => new
                {
                    o.CategoryId,
                    o.SubcategoryId,
                    o.OrganizationId,
                    o.PeriodId,
                })
                .Where(g => g.Sum(o => o.Amount) != 0)
                .Select(g => new
                {
                    g.Key.CategoryId,
                    g.Key.SubcategoryId,
                    g.Key.OrganizationId,
                    g.Key.PeriodId,
                    ReturnId = db.Returns().Where(r => r.CategoryId == g.Key.CategoryId && r.OrganizationId == g.Key.OrganizationId && r.PeriodId == g.Key.PeriodId).Select(r => r.Id as int?).FirstOrDefault(),
                    Amount = g.Sum(o => o.BucketId == BucketId.Overpayment ? -o.Amount : o.Amount),
                })
                .ToArrayAsync();

            var categoryLookup = await db.Categories()
                .Where(o => data.Select(d => d.CategoryId).Distinct().Contains(o.Id))
                .ToDictionaryAsync(o => o.Id);
            var orgLookup = await db.Organizations()
                .Where(o => data.Select(d => d.OrganizationId).Distinct().Contains(o.Id))
                .ToDictionaryAsync(o => o.Id);
            var periodLookup = await db.Periods()
                .Where(o => data.Select(d => d.PeriodId).Distinct().Contains(o.Id))
                .ToDictionaryAsync(o => o.Id);
            var returnLookup = await db.Returns()
                .Where(o => data.Select(d => d.ReturnId).Distinct().Contains(o.Id))
                .ToDictionaryAsync(o => o.Id);

            var reports = new List<StatementReport>();
            foreach (var org in data.GroupBy(o => orgLookup[o.OrganizationId]).OrderBy(o => o.Key.Id))
            {
                var report = new StatementReport();
                report.NoticeDate = noticeDate;
                report.OrganizationId = org.Key.Id;
                report.OrganizationName = org.Key.OrganizationName;
                report.Dba = org.Key.Dba;
                report.MulilineAddress = org.Key.MailingAddress?.MultilineAddress;

                foreach (var group in org.GroupBy(o => new { Period = periodLookup[o.PeriodId], o.CategoryId }).OrderBy(o => o.Key.Period.DueDate).ThenBy(o => o.Key.CategoryId))
                {
                    var period = group.Key.Period;
                    var category = categoryLookup[group.Key.CategoryId];

                    var scheduleItem = new StatementReport.StatementReportScheduleItem
                    {
                        DueDate = period.DueDate,
                        PeriodCovered = period.Id,
                        Category = category.Id,
                        Net = group.Where(o => o.SubcategoryId == SubcategoryId.Net).Sum(o => o.Amount),
                        Penalty = group.Where(o => o.SubcategoryId == SubcategoryId.Penalty).Sum(o => o.Amount),
                        Interest = group.Where(o => o.SubcategoryId == SubcategoryId.Interest).Sum(o => o.Amount),
                        TotalDue = group.Sum(o => o.Amount),
                    };
                    if (scheduleItem.TotalDue >= 0)
                    {
                        if (group.Max(o => o.ReturnId) is int returnId &&
                            returnLookup[returnId] is var ret)
                        {
                            if (ret.Filings.Any())
                            {
                                scheduleItem.ReasonCode = "Underpayment";
                            }
                            else
                            {
                                scheduleItem.ReasonCode = "Failure To File";
                            }
                        }
                        else
                        {
                            scheduleItem.ReasonCode = category.ToString();
                        }
                    }
                    else
                    {
                        scheduleItem.ReasonCode = "Overpayment";
                    }

                    report.Schedule.Add(scheduleItem);
                }

                reports.Add(report);
            }

            return reports;
        }
    }
}
