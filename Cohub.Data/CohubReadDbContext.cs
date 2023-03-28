using Cohub.Data.Pg;
using Microsoft.EntityFrameworkCore;
using SiteKit.Info;
using System;

namespace Cohub.Data
{
    public class CohubReadDbContext : CohubDbContextBase
    {
        [Obsolete("Design-time")]
        public CohubReadDbContext()
        {
        }

        public CohubReadDbContext(DbContextOptions<CohubReadDbContext> options, Actor actor) : base(options, actor)
        {
        }

        public DbSet<PgView> PgViews => Set<PgView>();
        public DbSet<Column> Columns => Set<Column>();
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<TablePrivilege> TablePrivileges => Set<TablePrivilege>();
    }
}
