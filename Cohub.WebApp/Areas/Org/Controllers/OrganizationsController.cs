using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Lic;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Org.Views.Organizations;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.ReviewLicenses)]
    [Area("Org")]
    [Route("org/organizations")]
    public class OrganizationsController : Controller
    {
        private readonly ILogger<OrganizationsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public OrganizationsController(ILogger<OrganizationsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Organization> query = db.Organizations()
                .IncludeReferences()
                .IncludeLabels();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Organization>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string contains = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, contains) ||
                        EF.Functions.ILike(o.OrganizationName, contains) ||
                        EF.Functions.ILike(o.Dba, contains) ||
                        EF.Functions.ILike(o.StatusId, token) ||
                        EF.Functions.ILike(o.ClassificationId, token) ||
                        EF.Functions.ILike(o.TypeId, token) ||
                        EF.Functions.ILike(o.StateID, token) ||
                        EF.Functions.ILike(o.FederalID, token) ||
                        o.Labels.Any(l => EF.Functions.ILike(l.Id, contains) || EF.Functions.ILike(l.Title, contains)) ||
                        EF.Functions.ILike(o.MailingAddress.FullAddress, contains) ||
                        EF.Functions.ILike(o.PhysicalAddress.FullAddress, contains) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => o.StatusId == OrganizationStatusId.Pending ? 0 : 1)
                .ThenByDescending(o => o.Id);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            string suggestedId = await db.Organizations().NextIdAsync();
            var input = new CreateOrganization
            {
                Id = suggestedId,
                StatusId = OrganizationStatusId.Pending,
                ClassificationId = null,
                TypeId = null,
            };

            return View(input);
        }

        [Authorize(Policy.Process)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(
                [FromServices] ReturnGenerator returnGenerator,
                CreateOrganization input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Organization organization = await input.AddOrganizationAsync(db);
                    await db.SaveChangesAsync();

                    await returnGenerator.GenerateMissingReturnsAsync(organizationIds: new[] { organization.Id });

                    return RedirectToAction(nameof(Details), new { organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("create-licensed-organization")]
        public async Task<IActionResult> CreateLicensedOrganization(
            [FromServices] ReturnGenerator returnGenerator,
            CreateLicensedOrganization input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
                input.Id = await db.Organizations().NextIdAsync();
                input.License.ExpirationDate = (await db.LicenseSettingsAsync()).CurrentBusinessLicenseExpirationDate;
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    Organization organization = await input.AddOrganizationAsync(db);
                    await db.SaveChangesAsync();

                    await returnGenerator.GenerateMissingReturnsAsync(organizationIds: new[] { organization.Id });

                    return RedirectToAction(nameof(Details), new { organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var organization = await db.Organizations(id)
                .IncludeCollections()
                .SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            await organization.LoadUserCommentsAsync();

            return View(organization);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }


        [Authorize(Policy.Process)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(string id, [FromForm] Organization input)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            organization.UpdateWith(input);

            if (ModelState.IsValid)
            {
                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Updated organization.");

                    return RedirectToAction("Details", new { id = organization.Id, sk_intent = "dismiss" });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(organization);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/change-id")]
        public async Task<IActionResult> ChangeId([FromRoute] string id, ChangeId input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            if (organization.Id == input.NewOrganizationId)
            {
                ModelState.AddModelError(nameof(input.NewOrganizationId), "The New Organization ID must be different.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        var changes = await db.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE sts.organization SET id = {input.NewOrganizationId} WHERE id = {id}"
                        );

                        db.Comment($"Changed organization ID from {id} to {input.NewOrganizationId}.", new OrganizationComment(input.NewOrganizationId));
                        await db.SaveChangesAsync();

                        await tx.CommitAsync();
                    }

                    TempData.Success("Changed organization ID.");

                    return RedirectToAction("Details", new { id = input.NewOrganizationId });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            ViewBag._Organization = organization;
            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/close")]
        public async Task<IActionResult> Close([FromRoute] string id, CloseOrganization input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            var organization = await db.Organizations(id).SingleOrDefaultAsync();
            if (organization == null)
            {
                return NotFound();
            }

            input.Organization = organization;

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    // This action is only available to a non-closed organization
                    if (organization.StatusId == OrganizationStatusId.Closed)
                    {
                        throw new ValidationException("The organization is already closed.");
                    }

                    // Set the expiration date of the organization's licenses to the closed date.
                    var activeLicenses = await db.Licenses()
                        .Where(o =>
                            o.OrganizationId == organization.Id &&
                            (o.TypeId == LicenseTypeId.Business || o.ExpirationDate > input.ClosedDate)
                        )
                        .ToListAsync();
                    foreach (var license in activeLicenses)
                    {
                        db.Comment($"Changed {organization.Id}'s license {license} expiration date from {license.ExpirationDate:d} to {input.ClosedDate:d}.", new OrganizationComment(organization.Id));
                        license.ExpirationDate = input.ClosedDate;
                    }

                    // Set the end date of filing schedules to the closed date.
                    var schedules = await db.FilingSchedules()
                        .Where(o => o.OrganizationId == organization.Id && o.EndDate > input.ClosedDate)
                        .ToListAsync();
                    foreach (var schedule in schedules)
                    {
                        db.Comment($"Changed {organization.Id}'s {schedule.PaymentChart} filing schedule end date from {schedule.EndDate:d} to {input.ClosedDate:d}.", new OrganizationComment(organization.Id));
                        schedule.EndDate = input.ClosedDate;
                    }

                    // Delete unfiled, payable and due returns that have Rev+Ovr balance of $0,
                    // and a start date that is after the closure date.
                    var returnStatuses = new[] { ReturnStatusId.Payable, ReturnStatusId.Due };
                    var buckets = new[] { BucketId.Revenue, BucketId.Overpayment };
                    var deletableReturns = await db.Returns()
                        .Where(r =>
                            r.OrganizationId == organization.Id &&
                            returnStatuses.Contains(r.StatusId) &&
                            r.Period.StartDate > input.ClosedDate &&
                            !r.Filings.Any() &&
                            0 == db.TransactionDetails().Where(o =>
                                o.OrganizationId == r.OrganizationId &&
                                o.CategoryId == r.CategoryId &&
                                o.PeriodId == r.PeriodId &&
                                buckets.Contains(o.BucketId)
                            ).Sum(o => o.Amount)
                        )
                        .ToListAsync();
                    foreach (var ret in deletableReturns)
                    {
                        db.Comment($"Deleted {organization.Id}'s unfiled {ret.PeriodId} {ret.CategoryId} return with a Rev+Ovr balance of $0.", new OrganizationComment(organization.Id));
                        db.Remove(ret);
                    }

                    // Delete draft and published statements
                    var statements = await db.Statements()
                        .Where(o => o.OrganizationId == organization.Id && new[] { StatementStatusId.Draft, StatementStatusId.Published }.Contains(o.StatusId))
                        .ToListAsync();
                    foreach (var statement in statements)
                    {
                        db.Comment($"Deleted {organization.Id}'s {statement.StatusId} statement #{statement.Id}.", new OrganizationComment(organization.Id));
                        db.Remove(statement);
                    }

                    // Change the organization's status to Closed
                    organization.StatusId = OrganizationStatusId.Closed;
                    db.Comment($"Closed {organization} organization using the closed date of {input.ClosedDate:d}.", new OrganizationComment(organization));

                    await db.SaveChangesAsync();


                    TempData.Success("Closed organization.");
                    return RedirectToAction("Details", new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Authorize(Policy.Process)]
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    db.Remove(organization);
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted organization.");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(organization);
        }


        [Authorize(Policy.ReviewLicenses)]
        [Route("{id}/add-comment")]
        public async Task<IActionResult> AddComment(string id, [Required] string commentText)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    db.UserComment(commentText, new OrganizationComment(organization));
                    await db.SaveChangesAsync();

                    TempData.Success("Added comment.");

                    return RedirectToAction("Details", new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to add comment.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = organization.Id });
        }


        [Authorize(Policy.Process)]
        [Route("{id}/add-label")]
        public async Task<IActionResult> AddLabel(string id, [Required, RegularExpression(".+:.+", ErrorMessage = "The Label must contain a colon that separates its group and value.")] string labelId)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    if (!organization.Labels.Any(o => o.Id == labelId))
                    {
                        organization.Labels.Add(await db.Labels().FindOrAddAsync(labelId, () => new Label { Id = labelId }));
                        db.Comment($"Added {labelId} label.", new OrganizationComment(organization));
                        await db.SaveChangesAsync();
                    }

                    TempData.Success("Added label.");

                    return RedirectToAction("Details", new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to add label.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = organization.Id });
        }


        [Authorize(Policy.Process)]
        [Route("{id}/remove-label")]
        public async Task<IActionResult> RemoveLabel(string id, [Required] string labelId)
        {
            var organization = await db.Organizations(id).SingleOrDefaultAsync();

            if (organization == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    if (organization.Labels.RemoveAll(o => o.Id == labelId) > 0)
                    {
                        db.Comment($"Removed {labelId} label.", new OrganizationComment(organization));
                        await db.SaveChangesAsync();
                    }

                    TempData.Success("Removed label.");

                    return RedirectToAction("Details", new { id = organization.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            TempData.Warn("Unable to remove label.");
            TempData.Error(ModelState);
            return RedirectToAction("Details", new { id = organization.Id });
        }
    }
}
