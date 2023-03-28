using Cohub.Data;
using Cohub.Data.Fin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Views.Shared.Components
{
    public class BatchTransactionsTableViewComponent : ViewComponent
    {
        private readonly CohubReadDbContext db;

        public BatchTransactionsTableViewComponent(CohubReadDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(int batchId)
        {
            var items = await db.Set<Transaction>()
                .Include(o => o.Details).ThenInclude(o => o.Period)
                .Where(o => o.BatchId == batchId)
                .OrderBy(o => o.Id)
                .ToArrayAsync();

            return View(items);
        }
    }
}
