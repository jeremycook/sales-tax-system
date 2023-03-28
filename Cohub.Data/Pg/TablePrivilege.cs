using System.ComponentModel.DataAnnotations.Schema;

namespace Cohub.Data.Pg
{
    [Table("table_privileges", Schema = "information_schema")]
    public class TablePrivilege
    {
        public string? Grantor { get; set; }
        public string? Grantee { get; set; }
        public string? TableCatalog { get; set; }
        public string? TableSchema { get; set; }
        public string? TableName { get; set; }
        public string? PrivilegeType { get; set; }
        public string? IsGrantable { get; set; }
        public string? WithHierarchy { get; set; }
    }
}
