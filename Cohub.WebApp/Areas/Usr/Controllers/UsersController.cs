using Cohub.Data;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Usr.Views.Users;
using Cohub.WebApp.Configuration;
using LinqKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteKit.Collections;
using SiteKit.Info;
using SiteKit.Jwt;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Usr.Controllers
{
    [Authorize(Policy.Super)]
    [Area("Usr")]
    [Route("usr/users")]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> logger;
        private readonly Actor actor;
        private readonly CohubDbContext db;

        public UsersController(ILogger<UsersController> logger, Actor actor, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.db = db;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<User> query = db.Users()
                .Include(o => o.Role);

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<User>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string contains = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Username, contains) ||
                        EF.Functions.ILike(o.Name, contains) ||
                        EF.Functions.ILike(o.RoleId, contains) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query
                .OrderBy(o => o.IsActive ? 0 : 1)
                .ThenByDescending(o => o.Id);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [HttpGet("[action]")]
        public ViewResult Access()
        {
            var ignoredDeclaringTypes = new[]
            {
                typeof(object),
                typeof(ControllerBase),
                typeof(Controller),
            };

            var model = typeof(Startup).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(c => c.GetMethods()
                    .Where(m => !ignoredDeclaringTypes.Contains(m.DeclaringType))
                    .Select(a => new
                    {
                        Area = c.GetCustomAttribute<AreaAttribute>()?.RouteValue,
                        Controller = Regex.Replace(c.Name, "Controller$", ""),
                        Action = a.Name,
                        AllowAnonymous = a.GetCustomAttribute<AllowAnonymousAttribute>() != null || c.GetCustomAttribute<AllowAnonymousAttribute>() != null,
                        Authorize = a.GetCustomAttributes<HttpMethodAttribute>()
                            .SelectMany(attr => attr.HttpMethods)
                            .DefaultIfEmpty("*")
                            .Select(m => new
                            {
                                Method = m,
                                Roles = a.GetCustomAttributes<AuthorizeAttribute>().Select(attr => Policy.Roles[attr.Policy]).Union(c.GetCustomAttributes<AuthorizeAttribute>().Select(attr => Policy.Roles[attr.Policy])),
                            })
                            .Distinct()

                    })
                )
                .SelectMany(o => o.Authorize.Select(a => new AccessModel
                {
                    Area = o.Area ?? "",
                    Controller = o.Controller,
                    Action = o.Action,
                    Method = a.Method,
                    //Policy = o.AllowAnonymous ? "" : a.Policy ?? Policy.Internal,
                    Roles = o.AllowAnonymous ? new[] { "Anyone" } : IntersectionOf(a.Roles),
                    //AuthenticationSchemes = a.AuthenticationSchemes ?? "",
                    AllowAnonymous = o.AllowAnonymous,
                }))
                .OrderBy(o => o.Area).ThenBy(o => o.Controller).ThenBy(o => o.Action).ThenBy(o => o.Method).ThenBy(o => o.Roles)
                .ToList();

            return View(model);
        }

        private string[] IntersectionOf(IEnumerable<IReadOnlyList<string>> roles)
        {
            if (!roles.Any())
            {
                roles = new[] { Policy.Roles[Policy.Internal] };
            }

            var intersection = roles.ElementAt(0).ToHashSet();
            foreach (var set in roles.Skip(1))
            {
                intersection.IntersectWith(set);
            }

            return intersection.ToArray();
        }

        [AllowAnonymous]
        [Route("switch-role")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SwitchRole(SwitchRoleModel input)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return NotFound();
            }

            var user = await db.Users(actor.UserId).SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }
            else if (user.RoleId != RoleId.Super)
            {
                // Only Super users can switch roles.
                return NotFound();
            }

            if (Request.IsGet())
            {
                ModelState.Clear();
                if (input.Role is null)
                {
                    input.Role = User.FindFirstValue(JwtClaimTypes.role);
                }
            }
            else if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    var claims = CohubWebAppStartupExtensions.CreateClaims(user);
                    claims.RemoveAll(o => o.Type == JwtClaimTypes.role);
                    claims.Add(new Claim(JwtClaimTypes.role, input.Role!));

                    var identity = new ClaimsIdentity(claims, User.Identity!.AuthenticationType, "name", "role");
                    var principal = new ClaimsPrincipal(identity);

                    // Sign out
                    foreach (string cookieName in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookieName);
                    }
                    // Sign in
                    await HttpContext.SignInAsync(principal);

                    logger.LogInformation("Switch principal role with ID {UserId} and username {Username} to {Role}.", user.Id, user.Username, user.Role);

                    return Redirect(Url.ReturnUrlOrAction("SwitchRole"));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            var input = new UserInputModel
            {
            };

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            UserInputModel input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = await input.AddUserAsync(db);

                    return RedirectToAction(nameof(Details), new { user.Id });
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
        public async Task<IActionResult> Details(int id)
        {
            var user = await db.Users(id)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await db.Users(id)
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            var input = new UserInputModel(user);
            return View(input);
        }

        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(int id, [FromForm] UserInputModel input)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users(id)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    return NotFound();
                }

                try
                {
                    await input.UpdateAsync(user, db);

                    TempData.Success("Updated user.");

                    return RedirectToAction("Details", new { id = user.Id });
                }
                catch (Exception ex)
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
            var user = await db.Users(id).SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            if (Request.IsPost() && ModelState.IsValid)
            {
                try
                {
                    db.Remove(user);
                    await db.SaveChangesAsync();

                    TempData.Success("Deleted user.");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(user);
        }
    }
}
