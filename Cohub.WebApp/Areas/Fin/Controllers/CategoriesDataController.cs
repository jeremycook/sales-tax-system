using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Fin")]
    [Route("fin/categories/data")]
    public class CategoriesDataController : Controller
    {
        private readonly CohubDbContext db;

        public CategoriesDataController(CohubDbContext db)
        {
            this.db = db;
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            IQueryable<Category> query = db.Categories()
                .OrderBy(o => o.Id);

            var list = await query
                .Select(o => new
                {
                    Id = o.Id,
                    Name = o.ToString(),
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("options")]
        public async Task<IActionResult> Options(string term, int top = 20)
        {
            term = (term ?? string.Empty).Trim();

            IQueryable<Category> query = db.Categories()
                .Where(o => EF.Functions.ILike(o.Id, term + "%"));

            query = query
                .OrderBy(o => o.Id);

            var list = await query
                .Take(top)
                .Select(o => new
                {
                    Value = o.Id,
                    Label = o.ToString(),
                    Text = o.ToString(),
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
