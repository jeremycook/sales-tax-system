using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Fin.Views.ReturnReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteKit.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Fin.Controllers
{
    [Authorize(Policy.Review)]
    [Area("Fin")]
    [Route("fin/return-report")]
    public class ReturnReportController : Controller
    {
        private readonly CohubDbContext db;

        public ReturnReportController(CohubDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult Index(IndexModel input)
        {
            return View(input);
        }

        [HttpGet("print")]
        public async Task<IActionResult> Print(IndexModel input, [FromQuery] Paging paging)
        {
            paging.RecordsPerPage = 1000;

            IOrderedQueryable<PrintModel.PrintRecordModel> query = db.Periods()
                .Where(p => input.StartDate <= p.EndDate && input.EndDate >= p.StartDate)
                .SelectMany(p =>
                    db.FilingSchedules()
                        .Where(fs =>
                            input.StartDate <= fs.EndDate && input.EndDate >= fs.StartDate &&
                            (
                                (!input.OrganizationIds.Any() && fs.Organization.SendPhysicalMail)
                                ||
                                (input.OrganizationIds.Any() && input.OrganizationIds.Contains(fs.OrganizationId))
                            ) &&
                            (!input.CategoryIds.Any() || input.CategoryIds.Contains(fs.PaymentChart.CategoryId)) &&

                            p.StartDate <= fs.EndDate && p.EndDate >= fs.StartDate &&

                            fs.PaymentChart.FrequencyId == p.FrequencyId
                        )
                        .Select(fs => new PrintModel.PrintRecordModel
                        {
                            OrganizationId = fs.OrganizationId,
                            CategoryId = fs.PaymentChart.CategoryId,
                            PeriodId = p.Id,
                            DueDate = p.DueDate,
                            OrganizationName = fs.Organization.OrganizationName,
                            Dba = fs.Organization.Dba,
                            PhysicalAddress = fs.Organization.PhysicalAddress.FullAddress,
                            OrganizationPhone = fs.Organization.OrganizationPhoneNumber,
                            OrganizationFax = null, // TODO: fs.Organization.OrganizationFaxNumber?
                        })
                )
                .OrderBy(o => o.OrganizationId).ThenBy(o => o.CategoryId).ThenBy(o => o.DueDate);

            paging.TotalRecords = await query.CountAsync();

            var model = new PrintModel
            {
                Records = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync(),
            };

            ViewBag._Paging = paging;
            return View(model);
        }
    }
}
