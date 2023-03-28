using Cohub.Data.Usr;
using Cohub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace Cohub.WebApp.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("~/org/organizations");
        }

        [AllowAnonymous]
        [Route("error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [Route("throw")]
        public IActionResult Throw()
        {
            if (User.IsInRole(RoleId.Super))
            {
                throw new Exception("Test throw endpoint.");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
