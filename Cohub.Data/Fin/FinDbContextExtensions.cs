using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.Data.Fin
{
    public static class FinDbContextExtensions
    {
        public static DbSet<Batch> Batches(this CohubDbContextBase db)
        {
            return db.Set<Batch>();
        }

        public static IQueryable<Batch> Batches(this CohubDbContextBase db, params int[] ids)
        {
            return db.Batches()
                .IncludeReferences()
                .IncludeTransactions()
                .AsSplitQuery()
                .Where(o => ids.Contains(o.Id)).OrderBy(o => o.Name);
        }

        public static IQueryable<Batch> IncludeReferences(this IQueryable<Batch> query)
        {
            return query
                .Include(o => o.CreatedBy);
        }

        /// <summary>
        /// Includes transactions and transaction details.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Batch> IncludeTransactions(this IQueryable<Batch> query)
        {
            return query
                .Include(o => o.Transactions).ThenInclude(o => o.Details);
        }

        public static DbSet<Bucket> Buckets(this CohubDbContextBase db)
        {
            return db.Set<Bucket>();
        }

        public static IQueryable<Bucket> Buckets(this CohubDbContextBase db, params string[] ids)
        {
            return db.Set<Bucket>().Where(o => ids.Contains(o.Id)).OrderBy(o => o.Name);
        }

        public static DbSet<Category> Categories(this CohubDbContextBase db)
        {
            return db.Set<Category>();
        }

        public static IQueryable<Category> Categories(this CohubDbContextBase db, params string[] ids)
        {
            return db.Set<Category>().Where(o => ids.Contains(o.Id)).OrderBy(o => o.Id);
        }

        public static DbSet<Filing> Filings(this CohubDbContextBase db)
        {
            return db.Set<Filing>();
        }

        public static IQueryable<Filing> IncludeReferences(this IQueryable<Filing> query)
        {
            return query.Include(o => o.Return);
        }

        public static IQueryable<Filing> Filings(this CohubDbContextBase db, params int[] ids)
        {
            return db.Filings().IncludeReferences().Where(o => ids.Contains(o.Id));
        }

        public static DbSet<Frequency> Frequencies(this CohubDbContextBase db)
        {
            return db.Set<Frequency>();
        }

        public static IQueryable<Frequency> Frequencies(this CohubDbContextBase db, params string[] ids)
        {
            return db.Frequencies().Include(o => o.Periods).Where(o => ids.Contains(o.Id));
        }

        public static DbSet<Subcategory> Subcategories(this CohubDbContextBase db)
        {
            return db.Set<Subcategory>();
        }

        public static IQueryable<Subcategory> Subcategories(this CohubDbContextBase db, params string[] ids)
        {
            return db.Set<Subcategory>().Where(o => ids.Contains(o.Id)).OrderBy(o => o.Id);
        }

        public static async Task<string> NextNameAsync(this DbSet<Batch> batches, string suffix = "")
        {
            var today = DateTime.Today;
            string contains = $"-{today:MM-dd-yyyy}-";

            var candidatesNames = await batches
                .Where(o => o.Name.Contains(contains))
                .Select(o => o.Name)
                .ToListAsync();

            candidatesNames.AddRange(batches.Local
                .Where(o => o.Name.Contains(contains))
                .Select(o => o.Name));

            int max = candidatesNames
                .Select(batchName => int.Parse(Regex.Matches(batchName, @"\d+").LastOrDefault()?.Value ?? "0"))
                .DefaultIfEmpty(0)
                .Max();

            int next = max + 1;

            string nextName = $"ST-{today:MM-dd-yyyy}-{next:D2}" + (suffix.IsNullOrWhiteSpace() ? "" : $"-{suffix}");
            return nextName;
        }

        public static DbSet<PaymentChart> PaymentCharts(this CohubDbContextBase db)
        {
            return db.Set<PaymentChart>();
        }

        public static IQueryable<PaymentChart> PaymentCharts(this CohubDbContextBase db, params int[] ids)
        {
            return db.Set<PaymentChart>()
                .IncludeReferences()
                .Include(o => o.Configurations)
                .Where(o => ids.Contains(o.Id));
        }

        public static IQueryable<PaymentChart> IncludeReferences(this IQueryable<PaymentChart> query)
        {
            return query
                .Include(o => o.Category)
                .Include(o => o.Frequency);
        }

        public static DbSet<FilingSchedule> FilingSchedules(this CohubDbContextBase db)
        {
            return db.Set<FilingSchedule>();
        }

        public static IQueryable<FilingSchedule> FilingSchedules(this CohubDbContextBase db, params int[] ids)
        {
            return db.FilingSchedules().IncludeReferences().Where(o => ids.Contains(o.Id));
        }

        public static IQueryable<FilingSchedule> IncludeReferences(this IQueryable<FilingSchedule> dbSet)
        {
            return dbSet
                .Include(o => o.Organization)
                .Include(o => o.PaymentChart);
        }

        public static DbSet<PaymentConfiguration> PaymentConfigurations(this CohubDbContextBase db)
        {
            return db.Set<PaymentConfiguration>();
        }

        public static IQueryable<PaymentConfiguration> PaymentConfigurations(this CohubDbContextBase db, params int[] ids)
        {
            return db.Set<PaymentConfiguration>().IncludeReferences().Where(o => ids.Contains(o.Id));
        }

        /// <summary>
        /// Includes <see cref="PaymentConfiguration.PaymentChart"/>.
        /// </summary>
        /// <param name="dbSet"></param>
        /// <returns></returns>
        public static IQueryable<PaymentConfiguration> IncludeReferences(this IQueryable<PaymentConfiguration> dbSet)
        {
            return dbSet
                .Include(o => o.PaymentChart);
        }

        public static DbSet<Period> Periods(this CohubDbContextBase db)
        {
            return db.Set<Period>();
        }

        public static IQueryable<Period> Periods(this CohubDbContextBase db, params string[] ids)
        {
            return db.Set<Period>().IncludeReferences().Where(o => ids.Contains(o.Id));
        }

        /// <summary>
        /// Includes <see cref="Period.Frequency"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Period> IncludeReferences(this IQueryable<Period> query)
        {
            return query
                .Include(o => o.Frequency);
        }

        public static DbSet<Return> Returns(this CohubDbContextBase db)
        {
            return db.Set<Return>();
        }

        public static IQueryable<Return> Returns(this CohubDbContextBase db, params int[] ids)
        {
            return db.Returns().IncludeReferences().Where(o => ids.Contains(o.Id));
        }

        /// <summary>
        /// Includes <see cref="Return.Category"/>, <see cref="Return.Organization"/>,
        /// <see cref="Period"/> and <see cref="Return.status"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Return> IncludeReferences(this IQueryable<Return> query)
        {
            return query
                .Include(o => o.Category)
                .Include(o => o.Organization)
                .Include(o => o.Period)
                .Include(o => o.Status);
        }

        /// <summary>
        /// Includes <see cref="Return.Labels"/> and <see cref="Return.Filings"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Return> IncludeCollections(this IQueryable<Return> query)
        {
            return query
                .Include(o => o.Labels)
                .Include(o => o.Filings)
                .AsSplitQuery();
        }

        /// <summary>
        /// Includes <see cref="Return.Comments"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Return> IncludeComments(this IQueryable<Return> query)
        {
            return query
                .Include(o => o.Comments).ThenInclude(o => o.Comment).ThenInclude(o => o.Author)
                .Include(o => o.Comments).ThenInclude(o => o.Comment).ThenInclude(o => o.OrganizationComments).ThenInclude(o => o.Organization)
                .Include(o => o.Comments).ThenInclude(o => o.Comment).ThenInclude(o => o.ReturnComments).ThenInclude(o => o.Return)
                .AsSplitQuery();
        }

        public static DbSet<Statement> Statements(this CohubDbContextBase db)
        {
            return db.Set<Statement>();
        }

        public static IQueryable<Statement> Statements(this CohubDbContextBase db, params int[] ids)
        {
            return db.Statements()
                .IncludeReferences()
                .IncludeCollections()
                .Where(o => ids.Contains(o.Id))
                .OrderByDescending(o => o.NoticeDate)
                .ThenBy(o => o.OrganizationId);
        }

        /// <summary>
        /// Includes <see cref="Statement.Organization"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Statement> IncludeReferences(this IQueryable<Statement> query)
        {
            return query
                .Include(o => o.Organization);
        }

        /// <summary>
        /// Includes <see cref="Statement.Dues"/> and <see cref="Statement.StatementComments"/>
        /// as split query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Statement> IncludeCollections(this IQueryable<Statement> query)
        {
            return query
                .Include(o => o.Dues)
                .Include(o => o.StatementComments)
                .AsSplitQuery();
        }

        public static DbSet<Transaction> Transactions(this CohubDbContextBase db)
        {
            return db.Set<Transaction>();
        }

        public static IQueryable<Transaction> Transactions(this CohubDbContextBase db, params int[] ids)
        {
            return db.Transactions()
                .IncludeReferences()
                .IncludeDetails()
                .AsSplitQuery()
                .Where(o => ids.Contains(o.Id));
        }

        /// <summary>
        /// Includes <see cref="Transaction.Batch"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Transaction> IncludeReferences(this IQueryable<Transaction> query)
        {
            return query
                .Include(o => o.Batch);
        }

        /// <summary>
        /// Includes <see cref="Transaction.Details"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Transaction> IncludeCollections(this IQueryable<Transaction> query)
        {
            return query
                .Include(o => o.Details);
        }

        /// <summary>
        /// Includes <see cref="Transaction.Details"/> with <see cref="TransactionDetail"/>'s references.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Transaction> IncludeDetails(this IQueryable<Transaction> query)
        {
            return query
                .Include(o => o.Details).ThenInclude(o => o.Bucket)
                .Include(o => o.Details).ThenInclude(o => o.Category)
                .Include(o => o.Details).ThenInclude(o => o.Organization)
                .Include(o => o.Details).ThenInclude(o => o.Period)
                .Include(o => o.Details).ThenInclude(o => o.Subcategory);
        }

        public static DbSet<TransactionDetail> TransactionDetails(this CohubDbContextBase db)
        {
            return db.Set<TransactionDetail>();
        }

        /// <summary>
        /// Includes <see cref="TransactionDetail.Organization"/>, <see cref="TransactionDetail.Category"/>, <see cref="TransactionDetail.Period"/>, <see cref="TransactionDetail.Bucket"/>, <see cref="TransactionDetail.Subcategory"/>, <see cref="TransactionDetail.Transaction"/>, .
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<TransactionDetail> IncludeReferences(this IQueryable<TransactionDetail> query)
        {
            return query
                .Include(o => o.Organization)
                .Include(o => o.Category)
                .Include(o => o.Bucket)
                .Include(o => o.Period)
                .Include(o => o.Subcategory)
                .Include(o => o.Transaction);
        }

    }
}
