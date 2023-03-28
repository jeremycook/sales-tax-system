using Cohub.Generator.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.Generator.Controllers
{
    [Route("api/directories")]
    [ApiController]
    public class ApiDirectoriesController : ControllerBase
    {
        private readonly IWebHostEnvironment env;

        public ApiDirectoriesController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public IEnumerable<LabeledValue> List(string? term)
        {
            var tokens = term?.Split(' ') ?? Array.Empty<string>();

            var basePath = Path.GetFullPath("../", env.ContentRootPath);
            return Directory.EnumerateDirectories(basePath, "*", SearchOption.AllDirectories)
                .Select(path => path.Substring(basePath.Length - 1).Replace('\\', '/'))
                .Where(d => !Regex.IsMatch(d, @"(/\.|/obj/|/bin/|/wwwroot/)"))
                .Where(d => !tokens.Any() || tokens.All(t => d.Contains(t, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(d => d)
                .Select(path => new LabeledValue(path, path));
        }
    }
}
