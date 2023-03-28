using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Org")]
    [Route("org/organization-contacts")]
    public class OrganizationContactsController : Controller
    {
        private readonly ILogger<OrganizationContactsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public OrganizationContactsController(ILogger<OrganizationContactsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<OrganizationContact> query = db.OrganizationContacts().IncludeReferences();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<OrganizationContact>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.LegalName, pattern) ||
                        EF.Functions.ILike(o.PhoneNumber, pattern) ||
                        EF.Functions.ILike(o.Email, pattern) ||
                        EF.Functions.ILike(o.OrganizationId, pattern) ||
                        EF.Functions.ILike(o.RelationshipId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderByDescending(o => o.Created);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [Authorize(Policy.Process)]
        [Route("create")]
        public async Task<IActionResult> Create(OrganizationContact input)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Comment($"Created organization contact {input}.", new OrganizationComment(input.OrganizationId));

                var organizationContact = new OrganizationContact(input);
                db.Add(organizationContact);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Created organization contact.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = organizationContact.Id }));
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
            var organizationContact = await db.OrganizationContacts(id).SingleOrDefaultAsync();

            if (organizationContact == null)
            {
                return NotFound();
            }

            return View(organizationContact);
        }


        [Authorize(Policy.Process)]
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var organizationContact = await db.OrganizationContacts(id).SingleOrDefaultAsync();

            if (organizationContact == null)
            {
                return NotFound();
            }

            return View(organizationContact);
        }


        [Authorize(Policy.Process)]
        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(OrganizationContact input)
        {
            var organizationContact = await db.OrganizationContacts(input.Id).SingleOrDefaultAsync();

            if (organizationContact == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                organizationContact.UpdateWith(input);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Updated organization contact.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = organizationContact.Id }));
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
        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var organizationContact = await db.OrganizationContacts(id).SingleOrDefaultAsync();

            if (organizationContact == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(organizationContact);
                db.Comment($"Deleted organization contact {organizationContact}.");

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted organization contact.");

                    return Redirect(Url.ReturnUrlOrAction("Index"));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(organizationContact);
        }


        [Authorize(Policy.Process)]
        [HttpGet("data/options")]
        public async Task<IActionResult> DataOptions(string term, int top = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(Enumerable.Empty<object>());
            }

            IQueryable<OrganizationContact> query = db.Set<OrganizationContact>();

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<OrganizationContact>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.LegalName, pattern) ||
                        EF.Functions.ILike(o.PhoneNumber, pattern) ||
                        EF.Functions.ILike(o.Email, pattern) ||
                        EF.Functions.ILike(o.OrganizationId, pattern) ||
                        EF.Functions.ILike(o.RelationshipId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => o.OrganizationId)
                .ThenBy(o => o.LegalName);

            var list = await query
                .Take(top)
                .Select(o => new
                {
                    Value = o.Id,
                    Label = o.ToString(),
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
