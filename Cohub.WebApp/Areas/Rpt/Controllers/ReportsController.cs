using Cohub.Data.Usr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Rpt.Controllers
{
    [Authorize(Policy.Super)]
    [Area("Rpt")]
    public class ReportsController : Controller
    {
        
    }
}
