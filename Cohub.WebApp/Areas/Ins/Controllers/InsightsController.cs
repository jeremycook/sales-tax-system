using Cohub.Data;
using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Ins.Controllers
{

    [Authorize(Policy.Super)]
    public class InsightsController : Controller
    {
        private readonly CohubReadDbContext db;

        public InsightsController(CohubReadDbContext db)
        {
            this.db = db;
        }
    }
}
