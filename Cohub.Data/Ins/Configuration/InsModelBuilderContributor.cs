using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Ins.Configuration
{
    public class InsModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueryDefinition>();
            {
                modelBuilder.Entity<QueryDefinition>().ToSqlQuery(
@"select t.table_name id, pg_get_viewdef(t.table_schema || '.' || t.table_name, true) as sql
from information_schema.""tables"" t
where t.table_schema = 'qry'
order by t.table_name");
            }
            modelBuilder.Entity<Report>();
        }
    }
}
