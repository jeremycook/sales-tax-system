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

namespace Cohub.WebApp.Areas.Usr.Controllers
{
    [Authorize(Policy.ReviewLicenses)]
    [Area("Usr")]
    [Route("usr/comments")]
    public class CommentsController : Controller
    {
        private readonly ILogger<CommentsController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public CommentsController(ILogger<CommentsController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging, int? userId, string? organizationId)
        {
            IQueryable<Comment> query = db.Comments().IncludeReferences();

            if (userId != null)
            {
                query = query.Where(o => o.AuthorId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(organizationId))
            {
                query = query.Where(o => o.OrganizationComments.Any(oc => oc.OrganizationId == organizationId));
            }

            if (!string.IsNullOrWhiteSpace(term))
            {
                var predicate = PredicateBuilder.New<Comment>();
                term.Split(' ').ToList().ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Text, pattern) ||
                        EF.Functions.ILike(o.Author.Username, token) ||
                        EF.Functions.ILike(o.Author.Name, token) ||
                        o.BatchComments.Any(r => EF.Functions.ILike(r.Batch.Name, token)) ||
                        o.OrganizationComments.Any(r => EF.Functions.ILike(r.OrganizationId, token)) ||
                        o.ReturnComments.Any(r => EF.Functions.ILike(r.Return.OrganizationId, token) || EF.Functions.ILike(r.Return.CategoryId, token) || EF.Functions.ILike(r.Return.PeriodId, token)) ||
                        o.StatementComments.Any(r => EF.Functions.ILike(r.Statement.OrganizationId, token) || r.Statement.Dues.Any(s => EF.Functions.ILike(s.CategoryId, token) || EF.Functions.ILike(s.PeriodId, token))) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderByDescending(o => o.Posted);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new Comment
            {
                IsUserComment = true,
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] Comment input, [FromQuery] string? organizationId, [FromQuery] int? returnId, [FromQuery] int? batchId, [FromQuery] int? statementId)
        {
            if (input.Html.IsNullOrWhiteSpace())
            {
                ModelState.AddModelError(nameof(input.Html), "The Comment field is required.");
            }

            if (ModelState.IsValid)
            {
                var comment = new Comment
                {
                    Html = Html.Sanitize(input.Html).ToString(),
                    Text = Html.Strip(input.Html),
                    Posted = DateTimeOffset.UtcNow,
                    AuthorId = actor.UserId,
                    IsUserComment = true,
                };

                if (organizationId != null)
                {
                    (new OrganizationComment(organizationId) as ICommentReference).ReferenceComment(comment);
                }
                if (returnId != null)
                {
                    (new ReturnComment(returnId.Value) as ICommentReference).ReferenceComment(comment);
                }
                if (batchId != null)
                {
                    (new BatchComment(batchId.Value) as ICommentReference).ReferenceComment(comment);
                }
                if (statementId != null)
                {
                    (new StatementComment(statementId.Value) as ICommentReference).ReferenceComment(comment);
                }
                db.Add(comment);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Created comment.");

                    if (organizationId != null)
                    {
                        return RedirectToAction("Details", "Organizations", new { area = "Org", id = organizationId });
                    }
                    else
                    {
                        return Redirect(Url.ReturnUrlOrAction("Details", new { id = comment.Id }));
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
            var comment = await db.Comments(id).SingleOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await db.Comments(id).SingleOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            if (!comment.IsUserComment)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited because it is not a user comment.");
            }

            if (comment.AuthorId != actor.UserId)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited because you are not the comment's author.");
            }

            if ((DateTime.Today - comment.Posted).TotalDays > 60)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited. It was posted more than 60 days ago.");
            }

            return View(comment);
        }


        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, [FromForm] Comment input)
        {
            var comment = await db.Comments(id).SingleOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            if (!comment.IsUserComment)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited because it is not a user comment.");
            }

            if (comment.AuthorId != actor.UserId)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited because you are not the comment's author.");
            }

            if ((DateTime.Today - comment.Posted).TotalDays > 60)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be edited. It was posted more than 60 days ago.");
            }

            if (input.Html.IsNullOrWhiteSpace())
            {
                ModelState.AddModelError(nameof(input.Html), "The Comment field is required.");
            }

            if (ModelState.IsValid)
            {
                comment.Html = Html.Sanitize(input.Html).ToString();
                comment.Text = Html.Strip(input.Html);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Updated comment.");

                    return Redirect(Url.ReturnUrlOrAction("Details", new { id = comment.Id }));
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [Route("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await db.Comments(id).SingleOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            if (!comment.IsUserComment)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be deleted because it is not a user comment.");
            }

            if (comment.AuthorId != actor.UserId)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be deleted because you are not the comment's author.");
            }

            if ((DateTime.Today - comment.Posted).TotalDays > 60)
            {
                ModelState.AddModelError(string.Empty, "The comment cannot be deleted. It was posted more than 60 days ago.");
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                db.Remove(comment);

                try
                {
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted comment.");

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(comment);
        }
    }
}
