using System.ComponentModel.DataAnnotations.Schema;

namespace Cohub.Data.Pg
{
    [Table("pg_view", Schema = "pg_catalog")]
    public class PgView
    {
        public string? Schemaname { get; set; }
        public string? Viewname { get; set; }
        public string? Viewowner { get; set; }
        public string? Definition { get; set; }
    }
}
