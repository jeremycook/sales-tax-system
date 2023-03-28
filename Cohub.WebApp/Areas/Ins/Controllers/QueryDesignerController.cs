using Cohub.Data;
using Cohub.Data.Ins;
using Cohub.Data.Usr;
using CsvHelper;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SiteKit.Collections;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Ins.Controllers
{
    [Authorize(Policy.Super)]
    [Area("Ins")]
    [Route("ins/query-designer")]
    public class QueryDesignerController : Controller
    {
        private readonly ILogger<QueryDesignerController> logger;
        private readonly InsightsDesignerDbContext db;
        private readonly CohubReadDbContext readDb;

        public QueryDesignerController(ILogger<QueryDesignerController> logger, InsightsDesignerDbContext db, CohubReadDbContext readDb)
        {
            this.logger = logger;
            this.db = db;
            this.readDb = readDb;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<QueryDefinition> query = db.Set<QueryDefinition>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var predicate = PredicateBuilder.New<QueryDefinition>();
                term.Split(' ').ToList().ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.Id, pattern) ||
                        EF.Functions.ILike(o.Sql, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.Id);

            paging.TotalRecords = await query.CountAsync();
            var list = await query.Skip(paging.Index).Take(paging.RecordsPerPage).ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }


        [HttpGet("create")]
        public async Task<IActionResult> Create(string? id = null)
        {
            var input = new QueryDefinition();

            if (id != null && await db.Set<QueryDefinition>().FindAsync(id) is QueryDefinition qd)
            {
                input.Sql = qd.Sql;
            }

            return View(input);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(QueryDefinition input)
        {
            if (ModelState.IsValid)
            {
                var sql = $"CREATE VIEW qry.{input.Id} AS \n\n{input.Sql}";

                try
                {
                    await db.Database.ExecuteSqlRawAsync(sql);

                    // Alert success

                    return RedirectToAction("Details", new { id = input.Id });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }

        // CREATE OR REPLACE VIEW

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var model = await db.Set<QueryDefinition>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Details(QueryDefinition input)
        {
            var model = await db.Set<QueryDefinition>()
                .SingleOrDefaultAsync(o => o.Id == input.Id);

            if (model == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var sql = $"CREATE OR REPLACE VIEW qry.{model.Id} AS\n{input.Sql}";

                try
                {
                    await db.Database.ExecuteSqlRawAsync(sql);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError("", ex.OriginalMessage());
                }
            }

            return View(input);
        }


        [HttpGet("{id}.csv")]
        public async Task<IActionResult> ExportCsv(string id, string? where = null, string? orderBy = null, int? offset = null, int? limit = null)
        {
            if (!await db.Set<QueryDefinition>().AnyAsync(o => o.Id == id))
            {
                return NotFound();
            }

            using var conn = new NpgsqlConnection(readDb.Database.GetConnectionString());
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM qry.{id}" +
                (where != null ? " WHERE " + where : "") +
                (orderBy != null ? " ORDER BY " + orderBy : "") +
                (offset != null ? " OFFSET " + offset : "") +
                (limit != null ? " LIMIT " + limit : "");
            using NpgsqlDataAdapter da = new(cmd);
            DataTable dataTable = new DataTable();

            await conn.OpenAsync();
            da.Fill(dataTable);

            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            foreach (DataColumn col in dataTable.Columns)
            {
                csv.WriteField(col.ColumnName);
            }
            csv.NextRecord();
            foreach (DataRow record in dataTable.Rows)
            {
                foreach (object? value in record.ItemArray)
                {
                    csv.WriteField(value);
                }
                csv.NextRecord();
            }

            return Content(writer.GetStringBuilder().ToString(), contentType: "text/csv");
        }
    }
}
