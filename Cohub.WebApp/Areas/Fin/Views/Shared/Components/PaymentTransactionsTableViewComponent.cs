using Cohub.Data;
using Cohub.Data.Fin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Views.Shared.Components
{
    public class PaymentTransactionsTableViewComponent : ViewComponent
    {
        private readonly CohubReadDbContext db;

        public PaymentTransactionsTableViewComponent(CohubReadDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(int depositTransactionDetailId)
        {
            var items = await db.Set<Transaction>()
                .Include(o => o.Batch)
                .Include(o => o.Details).ThenInclude(o => o.Period)
                .Where(o => o.Details.Any(d => d.Id == depositTransactionDetailId && d.BucketId == BucketId.Deposit))
                .OrderBy(o => o.Id)
                .ToArrayAsync();

            return View(items);
        }
    }
}
