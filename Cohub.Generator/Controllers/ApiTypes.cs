using Cohub.Generator.Models;
using Microsoft.AspNetCore.Mvc;
using SiteKit.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cohub.Generator.Controllers
{
    [Route("api/types")]
    [ApiController]
    public class ApiTypesController : ControllerBase
    {
        private readonly Assembly[] assemblies;

        public ApiTypesController(CandidateDbContextTypes types)
        {
            this.assemblies = types.Select(t => t.Assembly).Distinct().ToArray();
        }

        public IEnumerable<LabeledValue> List(string? term)
        {
            var tokens = term?.Split(' ') ?? Array.Empty<string>();

            var types = assemblies
                .SelectMany(a => a.ExportedTypes)
                .Select(t => t.FullName ?? string.Empty)
                .Where(fn => !tokens.Any() || tokens.All(t => fn.Contains(t, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(fn => fn);

            return types.Select(path => new LabeledValue(path, path));
        }
    }
}
