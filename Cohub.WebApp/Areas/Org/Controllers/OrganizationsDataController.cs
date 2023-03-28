using Cohub.Data;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Org")]
    [Route("org/organizations/data")]
    public class OrganizationsDataController : Controller
    {
        private readonly CohubDbContext db;

        public OrganizationsDataController(CohubDbContext db)
        {
            this.db = db;
        }

        [HttpGet("options")]
        public async Task<IActionResult> Options(string term, int limit = 20)
        {
            IQueryable<Organization> query = db.Organizations()
                .Where(o => o.StatusId != OrganizationStatusId.Closed || o.Id == term)
                .Where(o =>
                    EF.Functions.ILike(o.Id, $"%{term}%") ||
                    EF.Functions.ILike(o.OrganizationName, $"%{term}%") ||
                    EF.Functions.ILike(o.Dba, $"%{term}%")
                )
                .OrderBy(o =>
                    o.StatusId == OrganizationStatusId.Active ? 0 :
                    o.StatusId != OrganizationStatusId.Closed ? 1 :
                    2
                )
                .ThenBy(o => o.OrganizationName)
                .Take(limit);


            var list = await query
                .Select(o => new
                {
                    Value = o.Id,
                    Label = (o.StatusId == OrganizationStatusId.Closed ? "[Closed] " : "") + (!string.IsNullOrWhiteSpace(o.Dba) ? $"{o.Dba} ({o.OrganizationName})" : o.OrganizationName) + (o.StatusId != OrganizationStatusId.Closed ? " [" + o.StatusId + "]" : ""),
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string[]? id = null, string q = "", int limit = 20)
        {
            IQueryable<Organization> query;
            if (id?.Any() == true)
            {
                query = db.Organizations()
                    .Where(o => id.Contains(o.Id))
                    .OrderBy(o => o.Dba)
                    .ThenBy(o => o.OrganizationName);
            }
            else if (q == string.Empty)
            {
                query = db.Organizations()
                    .OrderByDescending(o => o.Updated)
                    .Take(limit)
                    .OrderBy(o => o.Dba)
                    .ThenBy(o => o.OrganizationName);
            }
            else
            {
                query = db.Organizations()
                    .Where(o =>
                        EF.Functions.ILike(o.Id, $"%{q}%") ||
                        EF.Functions.ILike(o.Dba, $"%{q}%") ||
                        EF.Functions.ILike(o.OrganizationName, $"%{q}%")
                    )
                    .OrderBy(o => o.Dba)
                    .ThenBy(o => o.OrganizationName)
                    .Take(limit);
            }

            var list = await query
                .Select(o => new
                {
                    o.Id,
                    Name = o.Id + " " + (!string.IsNullOrWhiteSpace(o.Dba) ? $"{o.Dba} ({o.OrganizationName})" : o.OrganizationName) + " [" + o.StatusId + "]",
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
