using Cohub.Data.Pg;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Cohub.WebApp.Areas.Rpt.Views.DataExplorer
{
    public class Details : TableId
    {
        public string[] Hidden { get; set; } = Array.Empty<string>();
        public string? Filters { get; set; }
        [Display(Name = "Order By")]
        public string[] OrderBy { get; set; } = Array.Empty<string>();
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 100;

        public Table? Table { get; set; }
        public Column[]? Columns { get; set; }
        public DataTable? DataTable { get; set; }
        public bool ReportExists { get; set; }

        public IEnumerable<KeyValuePair<string, string>> GenerateColumnTemplates(Column column, object? arg0 = null, object? arg1 = null)
        {
            var identifier = PgHelpers.Quote(column.ColumnName);

            switch (column.DataType)
            {
                case "text":
                case "character varying":
                    if (column.Nullable() && (arg0 == DBNull.Value || arg0 as string == string.Empty))
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"({identifier} IS NULL OR {identifier} ILIKE '')");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} ILIKE {PgHelpers.QuoteLiteral(arg0 ?? "text%")}");
                    }
                    break;
                case "date":
                    if (column.Nullable() && arg0 == DBNull.Value)
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} IS NULL");
                    }
                    else
                    {
                        var firstDate = arg0 as DateTime? ?? DateTime.Today.AddDays(-7);
                        var lastDate = arg1 as DateTime? ?? arg0 as DateTime? ?? DateTime.Today;
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} BETWEEN {PgHelpers.QuoteLiteral(firstDate.ToShortDateString())} AND {PgHelpers.QuoteLiteral(lastDate.ToShortDateString())}");
                    }
                    break;
                case "numeric":
                    if (column.Nullable() && arg0 == DBNull.Value)
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} IS NULL");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} BETWEEN {arg0 ?? "0"} AND {arg1 ?? arg0 ?? "100"}");
                    }
                    break;
                case "boolean":
                    if (column.Nullable() && arg0 == DBNull.Value)
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} IS NULL");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} = {(arg0 as bool?)?.ToString().ToLowerInvariant() ?? "true"}");
                    }
                    break;
                case "timestamp with time zone":
                    if (column.Nullable() && arg0 == DBNull.Value)
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} IS NULL");
                    }
                    else
                    {
                        var first = arg0 as DateTimeOffset? ?? DateTimeOffset.Now.AddDays(-7);
                        var last = arg1 as DateTimeOffset? ?? arg0 as DateTimeOffset? ?? DateTimeOffset.Now;
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} BETWEEN {PgHelpers.QuoteLiteral(first)} AND {PgHelpers.QuoteLiteral(last)}");
                    }
                    break;
                default:
                    if (column.Nullable() && arg0 == DBNull.Value)
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} IS NULL");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>($"{column.ColumnName}", $"{identifier} BETWEEN {PgHelpers.QuoteLiteral(arg0 ?? "first")}::{column.DataType} AND {PgHelpers.QuoteLiteral(arg0 ?? "last")}::{column.DataType}");
                    }
                    break;
            }
        }
    }
}
