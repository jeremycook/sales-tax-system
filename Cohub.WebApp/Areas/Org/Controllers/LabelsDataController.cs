using Cohub.Data;
using Cohub.Data.Org;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Org.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Org")]
    [Route("org/labels/data")]
    public class LabelsDataController : Controller
    {
        private readonly CohubDbContext db;

        public LabelsDataController(CohubDbContext db)
        {
            this.db = db;
        }


        [HttpGet("options")]
        public async Task<IActionResult> Options(string term, string? groupId = null, int top = 20)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Ok(Enumerable.Empty<object>());
            }

            var query = db.Labels()
                .Where(o => groupId == null || EF.Functions.ILike(o.GroupId, groupId))
                .Where(o =>
                    EF.Functions.ILike(o.Id, $"%{term}%") ||
                    EF.Functions.ILike(o.Title, $"%{term}%")
                )
                .OrderBy(o => o.IsActive ? 0 : 1)
                .ThenBy(o => o.Id);

            var list = await query
                .Take(top)
                .Select(o => new
                {
                    Value = o.Id,
                    Label = o.Title + (o.IsActive ? "" : " (Inactive)"),
                    Title = o.Description,
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
