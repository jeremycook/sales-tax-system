using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteKit.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Fin")]
    [Route("fin/periods/data")]
    public class PeriodsDataController : Controller
    {
        private readonly CohubDbContext db;

        public PeriodsDataController(CohubDbContext db)
        {
            this.db = db;
        }

        [HttpGet("options")]
        public async Task<IActionResult> Options(string term, int top = 20)
        {
            IQueryable<Period> query = db.Periods()
                .Where(o =>
                    (
                        o.Frequency.IsActive &&
                        o.StartDate <= DateTime.Today &&
                        o.DueDate >= DateTime.Today.AddYears(-1)
                    ) ||
                    (term != null && o.Id == term)
                );

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Period>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            var today = DateTime.Today;
            query = query
                .OrderBy(o => o.DueDate > today ? o.DueDate - today : today - o.DueDate)
                .ThenBy(o => o.EndDate);

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


        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown(string[]? id = null, string q = "", int limit = 20)
        {
            if (id?.Any() == true)
            {
                return Ok(await db.Periods()
                    .Where(o => id.Contains(o.Id))
                    .OrderBy(o => o.Id)
                    .Select(o => new
                    {
                        o.Id,
                        Name = o.Id,
                    })
                    .ToArrayAsync());
            }

            IQueryable<Period> query = db.Periods()
                .Where(o =>
                    (
                        o.Frequency.IsActive &&
                        o.StartDate <= DateTime.Today &&
                        o.DueDate >= DateTime.Today.AddYears(-1)
                    ) ||
                    (q != string.Empty && o.Id == q)
                );

            Tokenizer tokenizer = new Tokenizer(q);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Period>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Name, pattern) ||
                        EF.Functions.ILike(o.FrequencyId, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            var today = DateTime.Today;
            query = query
                .OrderBy(o => o.DueDate > today ? o.DueDate - today : today - o.DueDate)
                .ThenBy(o => o.EndDate);

            var list = await query
                .Take(limit)
                .Select(o => new
                {
                    Id = o.Id,
                    Name = o.Id,
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
