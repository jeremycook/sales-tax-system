using Cohub.Data;
using Cohub.Data.Fin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Views.Shared.Components
{
    public class TransactionsTableViewComponent : ViewComponent
    {
        private readonly CohubReadDbContext db;

        public TransactionsTableViewComponent(CohubReadDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string organizationId, string periodId, string categoryId)
        {
            var items = await db.Set<Transaction>()
                .Include(o => o.Batch)
                .Include(o => o.Details).ThenInclude(o => o.Period)
                .Where(o => o.Details.Any(td =>
                    (organizationId == "" || td.OrganizationId == organizationId) &&
                    (periodId == "" || td.PeriodId == periodId) &&
                    (categoryId == "" || td.CategoryId == categoryId)
                ))
                .OrderBy(o => o.Id)
                .ToArrayAsync();

            return View(items);
        }
    }
}
