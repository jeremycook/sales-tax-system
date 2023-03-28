using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Batches;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Extensions;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/returns")]
    public class ReturnsController : Controller
    {
        private readonly ILogger<ReturnsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public ReturnsController(ILogger<ReturnsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(Paging paging, string term, string? organizationId)
        {
            IQueryable<Return> returnsQuery;
            Tokenizer tokenizer = new Tokenizer(term);

            if (!organizationId.IsNullOrWhiteSpace())
            {
                returnsQuery = db.Returns().Where(o => o.OrganizationId == organizationId);
            }
            else if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Return>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string contains = $"%{token}%";
                    string startswith = $"{token}%";
                    string endswith = $"%{token}";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Status.Name, token) ||
                        EF.Functions.ILike(o.OrganizationId, contains) ||
                        EF.Functions.ILike(o.CategoryId, startswith) ||
                        EF.Functions.ILike(o.PeriodId, startswith) ||
                        EF.Functions.ILike(o.Organization.OrganizationName, contains) ||
                        EF.Functions.ILike(o.Organization.Dba, contains) ||
                        db.TransactionDetails().Any(td => tokenizer.Numbers.Contains(td.Amount) && td.OrganizationId == o.OrganizationId && td.CategoryId == o.CategoryId && td.PeriodId == o.PeriodId) ||
                        false
                    );
                });
                returnsQuery = db.Returns().AsExpandableEFCore().Where(predicate);
            }
            else
            {
                returnsQuery = db.Returns();
            }

            IDictionary<ReturnStatusId, object> stats = await returnsQuery.GroupBy(o => o.StatusId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => (object)g.Count);

            IQueryable<ReturnSummary> query = db.ReturnSummaries(returnsQuery)
                .OrderBy(o =>
                    o.Return.StatusId == ReturnStatusId.Due ? 0 :
                    1
                )
                .ThenByDescending(o => o.Period.DueDate)
                .ThenBy(o => o.Return.OrganizationId)
                .ThenBy(o => o.Return.CategoryId);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Tokenizer = tokenizer;
            ViewBag._Paging = paging;
            ViewBag._Stats = stats;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public IActionResult Create(string? organizationId)
        {
            var input = new Return
            {
                StatusId = ReturnStatusId.Payable,
                OrganizationId = organizationId,
                PeriodId = null,
                CategoryId = null,
            };

            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/transfer")]
        public async Task<IActionResult> Transfer(
                [FromServices] TransferReturnService service,
                int id,
                TransferReturn input)
        {
            var @return = await db.Returns().FindAsync(id);
            if (@return is null)
            {
                return NotFound();
            }

            if (Request.IsGet())
            {
                ModelState.Clear();

                input.ReturnId = id;
                input.DestinationOrganizationId = @return.OrganizationId;
                input.DestinationPeriodId = @return.PeriodId;
                input.DestinationCategoryId = @return.CategoryId;
            }
            else if (id != input.ReturnId)
            {
                throw new InvalidOperationException("The id and returnId must match.");
            }

            if (ModelState.IsValid && Request.IsPost())
            {
                try
                {
                    var batch = await service.ProcessAsync(input);

                    if (batch != null)
                    {
                        TempData.Success(Html.Interpolate($"Batch <a href=\"{Url.Action("Details", "Batches", new { id = batch.Id })}\">{batch}</a> opened."));
                    }

                    TempData.Success("Return transfered.");
                    return RedirectToAction("Details", new { id = input.ReturnId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            ViewBag.Return = @return;
            return View(input);
        }


        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(Return input, [FromQuery] string? organizationId)
        {
            Organization? organization = null;
            if (input.OrganizationId != null)
            {
                organization = await db.Organizations().IncludeLabels().SingleOrDefaultAsync(o => o.Id == input.OrganizationId);
                if (organization == null)
                {
                    ModelState.AddModelError(nameof(input.OrganizationId), "The Organization field does not match an organization.");
                }
            }

            if (ModelState.IsValid)
            {
                var ret = new Return(input)
                {
                    Labels = organization!.Labels.ToList(),
                };
                db.Add(ret);

                try
                {
                    db.Comment($"Created return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId));
                    await db.SaveChangesAsync();

                    TempData.Success($"Created return.");

                    if (organizationId == ret.OrganizationId)
                    {
                        return RedirectToAction("Details", "Organizations", new { area = "Org", id = ret.OrganizationId });
                    }
                    else
                    {
                        return RedirectToAction("Details", new { id = ret.Id });
                    }
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var ret = await db.Returns(id)
                .IncludeCollections()
                .IncludeComments()
                .SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            return View(ret);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var ret = await db.Returns(id).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            return View(ret);
        }


        [Authorize(Policy.Process)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, [FromForm] Return input)
        {
            var ret = await db.Returns(id).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //if (ret.ApplicationDate != input.ApplicationDate)
                //{
                //    db.Comment($"Changed return {ret} application date from {ret.ApplicationDate:d} to {input.ApplicationDate:d}.", new OrganizationComment(input.OrganizationId));
                //}

                ret.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success($"Saved return.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = ret.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/add-comment")]
        public async Task<IActionResult> AddComment(int id, [Required] string commentText)
        {
            var @return = await db.Returns(id).SingleOrDefaultAsync();

            if (@return == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    db.UserComment(commentText, new ReturnComment(@return), new OrganizationComment(@return.OrganizationId));
                    await db.SaveChangesAsync();

                    TempData.Success("Added comment.");

                    return RedirectToAction("Details", new { id = @return.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to add comment.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = @return.Id });
        }


        [Authorize(Policy.Process)]
        [Route("{id}/open")]
        public async Task<IActionResult> Open(int id)
        {
            var ret = await db.Returns(id).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            if (ret.StatusId != ReturnStatusId.Closed)
            {
                ModelState.AddModelError("", "Only closed returns can be opened.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                ret.StatusId = ret.Period.IsDue() ? ReturnStatusId.Due : ReturnStatusId.Payable;

                db.Comment($"Opened return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId));

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success(Html.Interpolate($"Opened <a href='{Url.Action("Details", new { id = ret.Id })}'>return</a>."));

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = ret.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(ret);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/close")]
        public async Task<IActionResult> Close(
                [FromServices] TransferMoneyService transferMoneyService,
                int id)
        {
            var ret = await db.Returns(id).SingleOrDefaultAsync();

            if (ret == null)
            {
                return NotFound();
            }

            if (ret.StatusId == ReturnStatusId.Closed)
            {
                ModelState.AddModelError("", "The return is already closed.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    using var tx = await db.Database.BeginTransactionAsync();

                    var batch = await transferMoneyService.ProcessAsync(new TransferMoneyInput
                    {
                        Action = TransferMoneyAction.AdjustDues,
                        OrganizationId = ret.OrganizationId,
                        CategoryId = ret.CategoryId,
                        PeriodId = ret.PeriodId,
                    });

                    if (batch is null)
                    {
                        db.Comment($"Closed return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId));
                    }
                    else
                    {
                        batch.Name = await db.Batches().NextNameAsync("CLOSED-RETURN");
                        batch.Note = $"Closed return {ret}.";
                        db.Comment($"Closed return {ret}.", new ReturnComment(ret), new OrganizationComment(ret.OrganizationId), new BatchComment(batch));
                    }
                    ret.StatusId = ReturnStatusId.Closed;
                    await db.SaveChangesAsync();

                    await tx.CommitAsync();

                    TempData.Success(Html.Interpolate($"Closed <a href='{Url.Action("Details", new { id = ret.Id })}'>return</a>."));
                    if (batch is not null)
                    {
                        TempData.Success(Html.Interpolate($"Created <a href='{Url.Action("Details", "Batches", new { id = batch.Id })}'>batch</a>."));
                    }

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = ret.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(ret);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(
                [FromServices] TransferMoneyService transferMoneyService,
                int id)
        {
            var ret = await db.Set<Return>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (ret == null)
            {
                return NotFound();
            }

            if (!(await db.TransactionDetails().Where(o =>
                o.BucketId == BucketId.Overpayment &&
                o.OrganizationId == ret.OrganizationId &&
                o.CategoryId == ret.CategoryId &&
                o.PeriodId == ret.PeriodId
            ).SumAsync(o => o.Amount)).IsZeroCents())
            {
                ModelState.AddModelError("", "This return cannot be deleted until its overpayment balance is transfered elsewhere or reduced to zero.");
            }

            if (!(await db.TransactionDetails().Where(o =>
                o.BucketId == BucketId.Revenue &&
                o.OrganizationId == ret.OrganizationId &&
                o.CategoryId == ret.CategoryId &&
                o.PeriodId == ret.PeriodId
            ).SumAsync(o => o.Amount)).IsZeroCents())
            {
                ModelState.AddModelError("", "This return cannot be deleted until its revenue balance is transfered elsewhere or reduced to zero.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    using var tx = await db.Database.BeginTransactionAsync();

                    var batch = await transferMoneyService.ProcessAsync(new TransferMoneyInput
                    {
                        Action = TransferMoneyAction.EraseDues,
                        OrganizationId = ret.OrganizationId,
                        CategoryId = ret.CategoryId,
                        PeriodId = ret.PeriodId,
                    });

                    if (batch is null)
                    {
                        db.Comment($"Deleted return {ret}.", new OrganizationComment(ret.OrganizationId));
                    }
                    else
                    {
                        batch.Name = await db.Batches().NextNameAsync("DELETED-RETURN");
                        batch.Note = $"Deleted return {ret}.";
                        batch.Post();
                        db.Comment($"Deleted return {ret}.", new OrganizationComment(ret.OrganizationId), new BatchComment(batch));
                    }

                    db.Remove(ret);

                    await db.SaveChangesAsync();

                    await tx.CommitAsync();

                    TempData.Success($"Deleted return.");
                    if (batch is not null)
                    {
                        TempData.Success(Html.Interpolate($"Created <a href='{Url.Action("Details", "Batches", new { id = batch.Id })}'>batch</a>."));
                    }

                    if (Url.ReturnUrl() is string returnUrl && !returnUrl.StartsWith("/fin/returns/"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(ret);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/add-label")]
        public async Task<IActionResult> AddLabel(int id, [Required, RegularExpression(".+:.+", ErrorMessage = "The Label must contain a colon that separates its group and value.")] string labelId)
        {
            var @return = await db.Returns(id).IncludeCollections().SingleOrDefaultAsync();

            if (@return == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    if (!@return.Labels.Any(o => o.Id == labelId))
                    {
                        @return.Labels.Add(await db.Labels().FindOrAddAsync(labelId, () => new Label { Id = labelId }));
                        db.Comment($"Added label {labelId}.", new ReturnComment(@return));
                        await db.SaveChangesAsync();
                    }

                    TempData.Success("Added label.");

                    return RedirectToAction("Details", new { id = @return.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to add label.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = @return.Id });
        }


        [Authorize(Policy.Process)]
        [Route("{id}/remove-label")]
        public async Task<IActionResult> RemoveLabel(int id, [Required] string labelId)
        {
            var @return = await db.Returns(id).IncludeCollections().SingleOrDefaultAsync();

            if (@return == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    if (@return.Labels.RemoveAll(o => o.Id == labelId) > 0)
                    {
                        db.Comment($"Removed label {labelId}.", new ReturnComment(@return));
                        await db.SaveChangesAsync();
                    }

                    TempData.Success("Removed label.");

                    return RedirectToAction("Details", new { id = @return.Id });
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to remove label.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = @return.Id });
        }
    }
}
