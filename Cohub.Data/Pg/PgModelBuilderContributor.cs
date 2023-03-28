using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Pg
{
    public class PgModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PgView>().ToTable("pg_views", "pg_catalog", b => b.ExcludeFromMigrations()).HasNoKey();

            modelBuilder.Entity<Column>().ToTable("columns", "information_schema", b => b.ExcludeFromMigrations()).HasNoKey();
            modelBuilder.Entity<Table>().ToTable("tables", "information_schema", b => b.ExcludeFromMigrations()).HasNoKey();
            modelBuilder.Entity<TablePrivilege>().ToTable("table_privileges", "information_schema", b => b.ExcludeFromMigrations()).HasNoKey();
        }
    }
}
