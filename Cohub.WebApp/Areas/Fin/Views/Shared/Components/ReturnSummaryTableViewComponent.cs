using Cohub.Data;
using Cohub.Data.Fin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Views.Shared.Components
{
    public class ReturnSummaryTableViewComponent : ViewComponent
    {
        private readonly CohubDbContext db;

        public ReturnSummaryTableViewComponent(CohubDbContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string organizationId)
        {
            var model = await db.ReturnSummaries(o => o.OrganizationId == organizationId && ReturnStatus.OpenIds.Contains(o.StatusId))
                .OrderBy(o => o.Period.DueDate)
                .ToListAsync();

            return View(model);
        }
    }
}
