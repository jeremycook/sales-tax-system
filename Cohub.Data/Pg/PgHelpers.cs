using System;

namespace Cohub.Data.Pg
{
    public static class PgHelpers
    {
        public static string Quote(string identifier)
        {
            return "\"" + identifier.Trim('"').Replace("\"", "\"\"") + "\"";
        }

        public static string? QuoteLiteral(object? literal)
        {
            return "'" + literal?.ToString().Trim('\'').Replace("\'", "''") + "'";
        }
    }
}
