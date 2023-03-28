using AngleSharp;
using AngleSharp.Dom;
using Cohub.Data;
using Cohub.Data.Configuration;
using Cohub.Data.Pg;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.FileExplorer.Services;
using Cohub.WebApp.Areas.Rpt.Views.DataExplorer;
using CsvHelper;
using Dapper;
using Fluid;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SiteKit.Collections;
using SiteKit.NPOI;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.Rpt.Controllers
{
    /// <summary>
    /// TODO: Authorize Policy.Internal and differentiate access based on the FileExplorerRole the User's Role is assigned.
    /// </summary>
    [Authorize(Policy.Review)]
    [Area("Rpt")]
    [Route("rpt/data-explorer")]
    public class DataExplorerController : Controller
    {
        private static readonly FluidParser _parser = new();

        private readonly ILogger<DataExplorerController> logger;
        private readonly CohubReadDbContext db;
        private readonly IOptions<CohubDataOptions> cohubDataOptions;
        private readonly FileExplorerService fileExplorerService;

        public DataExplorerController(ILogger<DataExplorerController> logger, CohubReadDbContext db, IOptions<CohubDataOptions> cohubDataOptions, FileExplorerService fileExplorerService)
        {
            this.logger = logger;
            this.db = db;
            this.cohubDataOptions = cohubDataOptions;
            this.fileExplorerService = fileExplorerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string term, Paging paging)
        {
            IQueryable<Table> query =
                from t in db.Tables
                join p in db.TablePrivileges.Where(o => o.Grantee == cohubDataOptions.Value.ReadRole) on new { t.TableCatalog, t.TableSchema, t.TableName } equals new { p.TableCatalog, p.TableSchema, p.TableName }
                select t;

            Tokenizer tokenizer = new Tokenizer(term);
            if (tokenizer.Any)
            {
                var predicate = PredicateBuilder.New<Table>();
                tokenizer.Tokens.ForEach(token =>
                {
                    string pattern = $"%{token}%";
                    predicate = predicate.And(o =>
                        EF.Functions.ILike(o.TableSchema, token) ||
                        EF.Functions.ILike(o.TableName, pattern) ||
                        false
                    );
                });
                query = query.AsExpandableEFCore().Where(predicate);
            }

            query = query.OrderBy(o => o.TableSchema).ThenBy(o => o.TableName);

            paging.TotalRecords = await query.CountAsync();
            var list = await query
                //.Skip(paging.Index).Take(paging.RecordsPerPage)
                .ToListAsync();

            ViewBag._Paging = paging;
            return View(list);
        }

        [HttpGet("{tableSchema}/{tableName}")]
        public async Task<IActionResult> Details(Details model)
        {
            if (!ModelState.IsValid || model.TableSchema == null || model.TableName == null)
            {
                return BadRequest(ModelState);
            }

            model.Table = await db.Set<Table>()
                .SingleOrDefaultAsync(o => o.TableSchema == model.TableSchema && o.TableName == model.TableName);

            if (model.Table == null)
            {
                return NotFound();
            }

            model.Columns = await db.Columns
                .Where(o => o.TableSchema == model.TableSchema && o.TableName == model.TableName)
                .ToArrayAsync();

            model.ReportExists = fileExplorerService.GetFileProvider().GetFileInfo($"/reports/{model.TableName}.liquid.html").Exists;

            using var conn = new NpgsqlConnection(db.Database.GetConnectionString());
            using var cmd = conn.CreateCommand();

            string whereClause = GenerateWhereClause(model);

            var sql = new StringBuilder();
            sql.AppendLine("SELECT\n\t" + string.Join(",\n\t", model.Columns.Select(o => o.ColumnName).Except(model.Hidden, StringComparer.OrdinalIgnoreCase).Select(PgHelpers.Quote)).TrimEnd('\n', '\t'));
            sql.AppendLine("FROM " + PgHelpers.Quote(model.TableSchema) + "." + PgHelpers.Quote(model.TableName));
            if (whereClause != string.Empty) sql.AppendLine(whereClause);
            if (model.OrderBy.Any()) sql.AppendLine("ORDER BY " + string.Join(", ", model.OrderBy.Intersect(model.Columns.Select(o => o.ColumnName).Concat(model.Columns.Select(o => o.ColumnName + " DESC")), StringComparer.OrdinalIgnoreCase).Select(o => o.Split(' ', 2)).Select(o => PgHelpers.Quote(o[0]) + " " + o.ElementAtOrDefault(1))));
            sql.AppendLine("OFFSET " + model.Offset);
            sql.AppendLine("LIMIT " + model.Limit);

            cmd.CommandText = sql.ToString();
            using NpgsqlDataAdapter da = new(cmd);
            model.DataTable = new DataTable();

            try
            {
                await conn.OpenAsync();
                da.Fill(model.DataTable);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.OriginalMessage()}");
                ModelState.AddModelError("", ex.AllMessages());
                ModelState.AddModelError("", cmd.CommandText);
            }

            return View(model);
        }

        private string GenerateWhereClause(Details model)
        {
            return model.Filters.IsNullOrWhiteSpace() ?
                string.Empty :
                $"WHERE\n{model.Filters}\n";
        }

        private string EncodeAsParameter(string columnName)
        {
            return Regex.Replace(columnName, @"[^a-zA-Z_0-9]", "_");
        }

        [HttpGet("{tableSchema}/{tableName}/column-options/{columnName}")]
        public async Task<IActionResult> ColumnOptions([Required] string tableSchema, [Required] string tableName, [Required] string columnName, string? q = null)
        {
            var column = await db.Columns
                .SingleOrDefaultAsync(o => o.TableSchema == tableSchema && o.TableName == tableName && o.ColumnName == columnName);

            if (column == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using var conn = new NpgsqlConnection(db.Database.GetConnectionString());
                await conn.OpenAsync();

                try
                {
                    string where, term;
                    if (q == null)
                    {
                        where = "";
                        term = "";
                    }
                    else if (column.IsString())
                    {
                        where = "WHERE " + PgHelpers.Quote(column.ColumnName) + " ILIKE @Term";
                        term = q.Contains("%") ? q : "%" + q + "%";
                    }
                    else
                    {
                        where = "WHERE " + PgHelpers.Quote(column.ColumnName) + " >= @Term::" + column.DataType;
                        term = q;
                    }

                    var results = await conn.QueryAsync($"SELECT id::text, text::text FROM (SELECT DISTINCT {PgHelpers.Quote(columnName)} \"id\", {PgHelpers.Quote(columnName)} \"text\" FROM {PgHelpers.Quote(tableSchema)}.{PgHelpers.Quote(tableName)} {where} ORDER BY {PgHelpers.Quote(columnName)} LIMIT 10) qry", new { Term = term });

                    return Ok(new
                    {
                        results
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    if (!q.IsNullOrWhiteSpace())
                    {
                        return Ok(new
                        {
                            results = new[] { new { id = q, text = q } }
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            results = Array.Empty<object>()
                        });
                    }
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet("{tableSchema}/{tableName}.{extension}")]
        public async Task<IActionResult> Export(Details model, [Required] string extension, [FromServices] IContentTypeProvider contentTypeProvider)
        {
            if (!ModelState.IsValid || model.TableSchema == null || model.TableName == null)
            {
                return BadRequest(ModelState);
            }

            model.Table = await db.Set<Table>()
                .SingleOrDefaultAsync(o => o.TableSchema == model.TableSchema && o.TableName == model.TableName);

            if (model.Table == null)
            {
                return NotFound();
            }

            model.Columns = await db.Columns
                .Where(o => o.TableSchema == model.TableSchema && o.TableName == model.TableName)
                .ToArrayAsync();

            if (!model.OrderBy.Any() && model.Table.TableType == TableTypeId.BaseTable)
            {
                model.OrderBy = new[] { model.Columns[0].ColumnName };
            }

            using var conn = new NpgsqlConnection(db.Database.GetConnectionString());
            using var cmd = conn.CreateCommand();

            string whereClause = GenerateWhereClause(model);

            var sql = new StringBuilder();
            sql.AppendLine("SELECT\n\t" + string.Join(",\n\t", model.Columns.Select(o => o.ColumnName).Except(model.Hidden, StringComparer.OrdinalIgnoreCase).Select(PgHelpers.Quote)).TrimEnd('\n', '\t'));
            sql.AppendLine("FROM " + PgHelpers.Quote(model.TableSchema) + "." + PgHelpers.Quote(model.TableName));
            if (whereClause != string.Empty) sql.AppendLine(whereClause);
            if (model.OrderBy.Any()) sql.AppendLine("ORDER BY " + string.Join(", ", model.OrderBy.Intersect(model.Columns.Select(o => o.ColumnName).Concat(model.Columns.Select(o => o.ColumnName + " DESC")), StringComparer.OrdinalIgnoreCase).Select(o => o.Split(' ', 2)).Select(o => PgHelpers.Quote(o[0]) + " " + o.ElementAtOrDefault(1))));
            // Limiting to the first 250K records
            sql.AppendLine("OFFSET 0");
            sql.AppendLine("LIMIT 250000");

            cmd.CommandText = sql.ToString();
            using NpgsqlDataAdapter da = new(cmd);
            model.DataTable = new DataTable();

            try
            {
                await conn.OpenAsync();
                da.Fill(model.DataTable);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.OriginalMessage()}");
                ModelState.AddModelError("", ex.AllMessages());
                ModelState.AddModelError("", cmd.CommandText);
                return BadRequest(ModelState);
            }

            switch (extension.ToLower())
            {
                case "csv":
                    {
                        using var writer = new StringWriter();
                        using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
                        foreach (DataColumn col in model.DataTable.Columns)
                        {
                            csv.WriteField(col.ColumnName);
                        }
                        csv.NextRecord();
                        foreach (DataRow record in model.DataTable.Rows)
                        {
                            foreach (object? value in record.ItemArray)
                            {
                                csv.WriteField(value);
                            }
                            csv.NextRecord();
                        }

                        return Content(writer.GetStringBuilder().ToString(), contentType: "text/csv");
                    }
                case "xlsx":
                case "xls":
                    {
                        var workbook = WorkbookHelpers.CreateWorkbook(model.DataTable, "." + extension);

                        byte[] data;
                        using (var stream = new MemoryStream())
                        {
                            workbook.Write(stream, leaveOpen: true);
                            data = stream.ToArray();
                        }

                        if (!contentTypeProvider.TryGetContentType("." + extension, out string contentType))
                        {
                            throw new ArgumentException($"The .{extension} file extension is not supported.", nameof(extension));
                        }

                        return File(data, contentType);
                    }
                case "html":
                    {
                        string liquidTemplate = System.IO.File.ReadAllText(fileExplorerService.GetFileModel($"/reports/{model.TableName}.liquid.html").PhysicalPath);

                        if (!_parser.TryParse(liquidTemplate, out var fluidTemplate, out var errorMessage))
                        {
                            ModelState.AddModelError("", $"Liquid template error: {errorMessage}");
                            return BadRequest(ModelState);
                        }

                        var records = model.DataTable.Rows.Cast<DataRow>()
                            .Select(row => model.DataTable.Columns.Cast<DataColumn>()
                                .ToDictionary(
                                    col => col.ColumnName,
                                    col => row[col] is object value && value != DBNull.Value ? value : null
                                )
                            );

                        TemplateContext context = new()
                        {
                            CultureInfo = CultureInfo.CurrentCulture,
                            TimeZone = TimeZoneInfo.Local,
                        };
                        context.SetValue("records", records);

                        string renderedHtml = fluidTemplate.Render(context);

                        var fileContents = Encoding.UTF8.GetBytes(renderedHtml);
                        return File(fileContents, "text/html");
                    }
                default:
                    return BadRequest($"The {extension} file extension is not supported.");
            }
        }
    }
}
