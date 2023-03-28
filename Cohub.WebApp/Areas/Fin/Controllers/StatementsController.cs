using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Deposits;
using Cohub.Data.Fin.Statements;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Fin.Views.Statements;
using Dapper;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Extensions;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/statements")]
    public class StatementsController : Controller
    {
        private readonly ILogger<StatementsController> logger;
        private readonly CohubDbContext db;
        private readonly StatementCalculator statementCalculator;
        private readonly DepositService depositService;

        public StatementsController(ILogger<StatementsController> logger, CohubDbContext db, StatementCalculator statementCalculator, DepositService depositService)
        {
            this.logger = logger;
            this.db = db;
            this.statementCalculator = statementCalculator;
            this.depositService = depositService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Paging paging, string term, bool showDetails = true, StatementStatusId? statementStatusId = null)
        {
            IQueryable<Statement> query = db.Statements().IncludeReferences();

            if (showDetails)
            {
                query = query.Include(o => o.Dues).AsSplitQuery();
            }

            if (statementStatusId.HasValue)
            {
                query = query.Where(o => o.StatusId == statementStatusId.Value);
            }

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Statement>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string contains = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.OrganizationId, contains) ||
                        EF.Functions.ILike(o.Organization.OrganizationName, contains) ||
                        EF.Functions.ILike(o.Organization.Dba, contains) ||

                        (token == "Draft" && o.StatusId == StatementStatusId.Draft) ||
                        (token == "Published" && o.StatusId == StatementStatusId.Published) ||
                        (token == "Archived" && o.StatusId == StatementStatusId.Archived) ||
                        (token == "Statement" && o.TypeId == StatementTypeId.Statement) ||
                        (token == "Assessment" && o.TypeId == StatementTypeId.Assessment) ||

                        tokenizer.Numbers.Contains(o.Id) ||
                        tokenizer.Numbers.Contains(o.OverpaymentBalance) ||
                        tokenizer.Numbers.Contains(o.GrandTotalDue) ||

                        tokenizer.Dates.Contains(o.NoticeDate) ||
                        (o.AssessmentDueDate != null && tokenizer.Dates.Contains(o.AssessmentDueDate.Value)) ||

                        o.Dues.Any(d =>
                            EF.Functions.ILike(d.CategoryId, token) ||
                            EF.Functions.ILike(d.PeriodId, token) ||
                            (token == "HasFiled" && d.HasFiled) ||
                            (token == "HasNotFiled" && !d.HasFiled) ||
                            tokenizer.Numbers.Contains(d.TotalDue) ||
                            false
                        ) ||

                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderByDescending(o => o.Updated)
                .ThenBy(o => o.OrganizationId);

            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            paging.TotalRecords = await query.CountAsync();

            ViewBag._Paging = paging;
            ViewBag._ShowDetails = showDetails;
            ViewBag._StatementStatusId = statementStatusId;
            return View(list);
        }

        [Authorize(Policy.Audit)]
        [Route("generate-statements")]
        public async Task<IActionResult> GenerateStatements(GenerateStatements input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
                input.OrganizationId ??= "*";
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                var generatedStatements = new List<Statement>();

                var draftStatements = await db.Statements()
                    .Where(o => o.StatusId == StatementStatusId.Draft)
                    .ToArrayAsync();

                var dueTransactionDetails = await db.TransactionDetails()
                    .Where(o =>
                        new[] { BucketId.Due, BucketId.Overpayment }.Contains(o.BucketId) &&
                        (input.OrganizationId == "*" || o.OrganizationId == input.OrganizationId) &&
                        o.Period.EndDate <= input.MaxEndDate
                    )
                    .GroupBy(o => new
                    {
                        o.OrganizationId,
                        o.CategoryId,
                        o.PeriodId,
                    })
                    .Select(g => new
                    {
                        g.Key.OrganizationId,
                        g.Key.CategoryId,
                        g.Key.PeriodId,
                        HasFiled = db.Filings().Any(f =>
                            f.Return.OrganizationId == g.Key.OrganizationId &&
                            f.Return.CategoryId == g.Key.CategoryId &&
                            f.Return.PeriodId == g.Key.PeriodId
                        ),
                        NetDue = g.Sum(o => o.BucketId == BucketId.Due && o.SubcategoryId == SubcategoryId.Net ? o.Amount : 0),
                        PenaltyDue = g.Sum(o => o.BucketId == BucketId.Due && o.SubcategoryId == SubcategoryId.Penalty ? o.Amount : 0),
                        InterestDue = g.Sum(o => o.BucketId == BucketId.Due && o.SubcategoryId == SubcategoryId.Interest ? o.Amount : 0),
                        TotalDue = g.Sum(o => o.BucketId == BucketId.Due ? o.Amount : 0),
                        TotalOverpayment = g.Sum(o => o.BucketId == BucketId.Overpayment ? o.Amount : 0),
                    })
                    .Where(o => o.TotalDue != 0 || o.TotalOverpayment != 0)
                    .ToListAsync();

                var warnings = new List<string>();

                foreach (var orgId in dueTransactionDetails.Select(o => o.OrganizationId).ToArray())
                {
                    if (draftStatements.Where(o => o.OrganizationId == orgId) is var matchingStatements &&
                        matchingStatements.Any())
                    {
                        if (input.ReplaceMatchingDrafts)
                        {
                            foreach (var item in matchingStatements)
                            {
                                db.Remove(item);
                            }
                        }
                        else
                        {
                            warnings.Add($"A new statement was not generated for the {orgId} organization since it already has a draft statement or assessment, and the Replace Matching Drafts option was not enabled.");
                            continue;
                        }
                    }

                    var statement = new Statement
                    {
                        OrganizationId = orgId,
                        NoticeDate = input.NoticeDate,
                        TypeId = StatementTypeId.Statement,
                        StatusId = StatementStatusId.Draft,
                        Dues = new(),
                    };

                    // Add statements for dues that do not have a return
                    var matchingDues = dueTransactionDetails.Where(o => o.OrganizationId == orgId).ToList();
                    foreach (var due in matchingDues)
                    {
                        var statementDetail = new StatementDue
                        {
                            CategoryId = due.CategoryId,
                            PeriodId = due.PeriodId,
                            HasFiled = due.HasFiled,
                            DueDate = input.NoticeDate,
                            NetDue = due.NetDue,
                            PenaltyDue = due.PenaltyDue,
                            InterestDue = due.InterestDue,
                            TotalDue = due.TotalDue,
                            TotalOverpayment = due.TotalOverpayment,
                        };

                        statement.Dues.Add(statementDetail);

                        dueTransactionDetails.Remove(due);
                    }

                    generatedStatements.Add(statement);
                }

                if (dueTransactionDetails.Any())
                {
                    throw new InvalidOperationException($"The {nameof(dueTransactionDetails)} list must be empty at this point.");
                }

                foreach (var statement in generatedStatements.ToArray())
                {
                    statement.Dues.RemoveAll(s => s.TotalDue.IsZeroCents() && s.TotalOverpayment.IsZeroCents());
                    statement.Recalculate();

                    if (statement.GrandTotalDue != 0 || statement.OverpaymentBalance >= input.MinimumOverpayment)
                    {
                        db.Add(statement);
                        db.Comment($"Generated statement.", new StatementComment(statement), new OrganizationComment(statement.OrganizationId));
                    }
                    else
                    {
                        generatedStatements.Remove(statement);
                    }
                }

                if (generatedStatements.Any())
                {
                    try
                    {
                        await db.SaveChangesAsync();

                        TempData.Success($"Generated {generatedStatements.Count} statements.");
                        warnings.ForEach(o => TempData.Warn(o));

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", ex.AllMessages());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No statements were generated.");
                    warnings.ForEach(o => ViewData.Warn(o));
                }
            }

            return View(input);
        }

        [Authorize(Policy.Audit)]
        [Route("delete-drafts")]
        public async Task<IActionResult> DeleteDrafts()
        {
            var draftStatements = await db.Statements()
                .IncludeReferences()
                .IncludeCollections()
                .Where(o => o.StatusId == StatementStatusId.Draft)
                .ToListAsync();

            if (Request.IsPost())
            {
                if (!draftStatements.Any())
                {
                    ModelState.AddModelError("", "No drafts to delete.");
                }

                if (ModelState.IsValid)
                {

                    try
                    {
                        db.RemoveRange(draftStatements);
                        await db.SaveChangesAsync();

                        TempData.Success($"Deleted {draftStatements.Count} draft statements.");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", ex.AllMessages());
                    }
                }
            }

            return View(draftStatements.Count);
        }

        [Route("publish-drafts")]
        public async Task<IActionResult> PublishDrafts()
        {
            var draftStatements = await db.Statements()
                .Where(o => o.StatusId == StatementStatusId.Draft)
                .ToListAsync();

            if (Request.IsPost())
            {
                var warnings = new List<string>();
                if (!draftStatements.Any())
                {
                    ModelState.AddModelError("", "No drafts to publish.");
                }
                else
                {
                    var relatedPublishedStatements = await db.Statements()
                        .Where(o => o.TypeId == StatementTypeId.Statement && o.StatusId == StatementStatusId.Published)
                        .Where(o => draftStatements.Select(s => s.OrganizationId).Contains(o.OrganizationId))
                        .ToListAsync();

                    if (relatedPublishedStatements.Any())
                    {
                        relatedPublishedStatements.ForEach(o => o.StatusId = StatementStatusId.Archived);
                        warnings.Add($"Archived {relatedPublishedStatements.Count} previously published statements.");
                    }

                    var relatedPublishedAssessments = await db.Statements()
                        .Where(o => o.TypeId == StatementTypeId.Assessment && o.StatusId == StatementStatusId.Published)
                        .Where(o => draftStatements.Select(s => s.OrganizationId).Contains(o.OrganizationId))
                        .ToListAsync();

                    if (relatedPublishedAssessments.Any())
                    {
                        draftStatements.RemoveAll(o => relatedPublishedAssessments.Select(s => s.OrganizationId).Contains(o.OrganizationId));
                        warnings.Add($"{relatedPublishedAssessments.Count} previously published assessments already exist and were skipped. If you would like to publish a new statement or assessment for an organization then you will need to archive that organization's existing assessment first.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var entityType = db.Model.FindEntityType<Statement>();

                    try
                    {
                        foreach (var statement in draftStatements)
                        {
                            statement.StatusId = StatementStatusId.Published;
                            db.Comment($"Published statement.", new StatementComment(statement), new OrganizationComment(statement.OrganizationId));
                        }

                        await db.SaveChangesAsync();

                        warnings.ForEach(o => TempData.Warn(o));
                        TempData.Success($"Published {draftStatements.Count} statements.");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", ex.AllMessages());
                    }
                }
            }

            return View(draftStatements.Count);
        }

        [HttpGet("print-published")]
        public async Task<IActionResult> PrintPublished()
        {
            var data = await db.Statements()
                .IncludeReferences()
                .IncludeCollections()
                .Where(o => o.StatusId == StatementStatusId.Published)
                .OrderBy(o => o.OrganizationId)
                .ToListAsync();

            var reports = new List<StatementReport>();

            foreach (var item in data)
            {
                var report = new StatementReport();
                report.NoticeDate = item.NoticeDate;
                report.TypeId = item.TypeId;
                report.AssessmentDueDate = item.AssessmentDueDate;
                report.OrganizationId = item.Organization.Id;
                report.OrganizationName = item.Organization.OrganizationName;
                report.Dba = item.Organization.Dba;
                report.MulilineAddress = item.Organization.MailingAddress.MultilineAddress;

                foreach (var group in item.Dues.OrderBy(o => o.DueDate).ThenBy(o => o.CategoryId))
                {
                    var scheduleItem = new StatementReport.StatementReportScheduleItem
                    {
                        DueDate = group.DueDate,
                        PeriodCovered = group.PeriodId,
                        Category = group.CategoryId,
                        Net = group.NetDue,
                        Penalty = group.PenaltyDue,
                        Interest = group.InterestDue,
                        TotalDue = group.TotalDue,
                        TotalOverpayment = group.TotalOverpayment,
                        ReasonCode =
                            group.CategoryId == CategoryId.NSFFee ? "N" :
                            group.CategoryId == CategoryId.LicenseFee ? "L" :
                            !group.TotalOverpayment.IsZeroCents() && group.TotalDue.IsZeroCents() ? "O" :
                            group.HasFiled ? "U" :
                            "F",
                    };
                    report.Schedule.Add(scheduleItem);
                }

                reports.Add(report);
            }

            return View(reports);
        }

        [Authorize(Policy.Audit)]
        [Route("archived-published-statements")]
        public async Task<IActionResult> ArchivePublishedStatements()
        {
            var publishedStatements = await db.Statements()
                .Where(o => o.StatusId == StatementStatusId.Published && o.TypeId == StatementTypeId.Statement)
                .ToListAsync();

            if (Request.IsPost())
            {
                if (!publishedStatements.Any())
                {
                    ModelState.AddModelError("", "No published statements to archive.");
                }

                if (ModelState.IsValid)
                {

                    try
                    {
                        foreach (var statement in publishedStatements)
                        {
                            statement.StatusId = StatementStatusId.Archived;
                            db.Comment($"Archived statement.", new StatementComment(statement), new OrganizationComment(statement.OrganizationId));
                        }
                        await db.SaveChangesAsync();

                        TempData.Success($"Archived {publishedStatements.Count} published statements.");

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", ex.AllMessages());
                    }
                }
            }

            return View(publishedStatements.Count);
        }

        [HttpGet("preview")]
        public async Task<IActionResult> Preview(DateTime? noticeDate)
        {
            noticeDate ??= DateTime.Today;

            var model = await statementCalculator.CalculateStatementReportsAsync(noticeDate.Value);

            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var statement = await db.Statements(id).SingleOrDefaultAsync();

            if (statement == null)
            {
                return NotFound();
            }

            return View(statement);
        }

        [Authorize(Policy.Audit)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var statement = await db.Statements(id).SingleOrDefaultAsync();

            if (statement == null)
            {
                return NotFound();
            }

            return View(statement);
        }


        [Authorize(Policy.Audit)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit([FromRoute] int id, Statement input)
        {
            var statement = await db.Statements(id).SingleOrDefaultAsync();

            if (statement == null)
            {
                return NotFound();
            }

            if (statement.StatusId != StatementStatusId.Draft)
            {
                ModelState.AddModelError("", "Only drafts can be modified.");
            }

            if (ModelState.IsValid)
            {
                statement.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Statement updated.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = statement.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }

        [Authorize(Policy.Audit)]
        [Route("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            var statement = await db.Statements(id).SingleOrDefaultAsync();

            if (statement == null)
            {
                return NotFound();
            }

            if (statement.StatusId != StatementStatusId.Published)
            {
                ModelState.AddModelError("", "Only published statements and assessments can be archived.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    statement.StatusId = StatementStatusId.Archived;
                    db.Comment($"Archived statement.", new StatementComment(statement), new OrganizationComment(statement.OrganizationId));
                    await db.SaveChangesAsync();

                    TempData.Success("Archived statement.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.AllMessages());
                }
            }

            return View(statement);
        }

        [Authorize(Policy.Audit)]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var statement = await db.Statements(id).SingleOrDefaultAsync();

            if (statement == null)
            {
                return NotFound();
            }

            if (statement.StatusId != StatementStatusId.Draft)
            {
                ModelState.AddModelError("", "Only draft statements can be modified.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                var entityType = db.Model.FindEntityType<Statement>();

                try
                {
                    await db.Database.GetDbConnection().ExecuteAsync($"DELETE FROM {entityType.GetSchemaQualifiedTableName()} WHERE \"id\" = @Id", new { Id = id });

                    db.Comment($"Deleted statement #{id}.", new OrganizationComment(statement.OrganizationId));
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted statement.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.AllMessages());
                }
            }

            return View(statement);
        }
    }
}
