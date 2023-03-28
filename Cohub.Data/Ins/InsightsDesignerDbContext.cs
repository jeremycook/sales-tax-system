using Microsoft.EntityFrameworkCore;
using SiteKit.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cohub.Data.Ins
{
    public class InsightsDesignerDbContext : CohubDbContextBase
    {
        [Obsolete("Design-time")]
        public InsightsDesignerDbContext()
        {
        }

        public InsightsDesignerDbContext(DbContextOptions<InsightsDesignerDbContext> options, Actor actor) : base(options, actor)
        {
        }
    }
}
