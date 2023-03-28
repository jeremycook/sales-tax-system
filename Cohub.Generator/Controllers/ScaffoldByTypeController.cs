using Cohub.Generator.Models;
using Cohub.Generator.Views.ScaffoldSubclasses;
using Microsoft.AspNetCore.Mvc;
using SiteKit.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cohub.Generator.Controllers
{
    [Route("[controller]")]
    public class ScaffoldByTypeController : Controller
    {
        private readonly EntityTypeProvider entityTypeProvider;
        private readonly CandidateDbContextTypes types;

        public ScaffoldByTypeController(EntityTypeProvider entityTypeProvider, CandidateDbContextTypes types)
        {
            this.entityTypeProvider = entityTypeProvider;
            this.types = types;
        }

        public IActionResult Index(ScaffoldByTypeInput input)
        {
            input ??= new ScaffoldByTypeInput();

            if (ModelState.IsValid)
            {
                try
                {
                    var type = types
                        .Select(t => t.Assembly)
                        .Distinct()
                        .SelectMany(o => o.ExportedTypes)
                        .Single(o => o.FullName == input.SourceTypeFullNamePattern);
                    input.SetEntityType(entityTypeProvider.GetEntityType(type));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.ToString());
                }
            }

            return View(input);
        }
    }
}
