using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Returns
{
    /// <summary>
    /// Generate missing <see cref="ReturnStatusId.Payable"/>
    /// and <see cref="ReturnStatusId.Due"/> <see cref="Return"/>s.
    /// </summary>
    public class ReturnGenerator
    {
        private readonly ILogger<ReturnGenerator> logger;
        private readonly CohubDbContext db;

        public ReturnGenerator(ILogger<ReturnGenerator> logger, CohubDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }

        /// <summary>
        /// Calculates any missing returns from <paramref name="startDate"/> through <paramref name="endDate"/> for <paramref name="organizationIds"/>.
        /// If <paramref name="organizationIds"/> is empty all organizations will be considered.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="organizationIds"></param>
        /// <returns></returns>
        public async Task<List<Return>> CalculateMissingReturnsAsync(DateTime startDate, DateTime endDate, IEnumerable<string> organizationIds)
        {
            var missingReturns = await db.Periods()
                .Where(p => startDate <= p.EndDate && endDate >= p.StartDate)
                .SelectMany(p =>
                    db.FilingSchedules()
                        .Where(pp =>
                            startDate <= pp.EndDate && endDate >= pp.StartDate &&
                            p.StartDate <= pp.EndDate && p.EndDate >= pp.StartDate &&
                            (!organizationIds.Any() || organizationIds.Contains(pp.OrganizationId)) &&

                            pp.PaymentChart.FrequencyId == p.FrequencyId &&
                            !db.Returns().Any(r => r.OrganizationId == pp.OrganizationId && r.CategoryId == pp.PaymentChart.CategoryId && r.PeriodId == p.Id)
                        )
                        .Select(pp => new Return
                        {
                            OrganizationId = pp.OrganizationId,
                            CategoryId = pp.PaymentChart.CategoryId,
                            PeriodId = p.Id,
                            StatusId = p.DueDate < DateTime.Today ? ReturnStatusId.Due : ReturnStatusId.Payable,
                            Labels = pp.Organization.Labels,
                        })
                )
                .ToListAsync();

            missingReturns = missingReturns
                .GroupBy(o => new
                {
                    o.OrganizationId,
                    o.CategoryId,
                    o.PeriodId,
                })
                .Select(g => g.First())
                .ToList();

            return missingReturns;
        }

        /// <summary>
        /// Generates any missing returns from <paramref name="startDate"/> through <paramref name="endDate"/> for <paramref name="organizationIds"/>.
        /// If <paramref name="organizationIds"/> is empty all organizations will be considered.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Return>> GenerateMissingReturnsAsync(DateTime startDate, DateTime endDate, IEnumerable<string> organizationIds)
        {
            var missingReturns = await CalculateMissingReturnsAsync(startDate, endDate, organizationIds);

            foreach (var ret in missingReturns)
            {
                db.Add(ret);
                db.Comment($"Generated return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId));
            }
            await db.SaveChangesAsync();

            // Note: A freshly generated return will not have been filed, so don't calculate dues.

            return missingReturns;
        }

        /// <summary>
        /// Generates any missing returns from 3 years ago plus current through today for <paramref name="organizationIds"/>.
        /// If <paramref name="organizationIds"/> is empty all organizations will be considered.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Return>> GenerateMissingReturnsAsync(IEnumerable<string> organizationIds)
        {
            return await GenerateMissingReturnsAsync(DateTime.SpecifyKind(new DateTime(DateTime.Today.Year - 3, 1, 1), DateTimeKind.Local), DateTime.Today, organizationIds);
        }

        /// <summary>
        /// Generates any missing returns from 3 years ago plus current through today.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Return>> GenerateMissingReturnsAsync()
        {
            return await GenerateMissingReturnsAsync(Array.Empty<string>());
        }
    }
}
