using Cohub.Data.Org;
using Cohub.Data.Usr;
using Microsoft.EntityFrameworkCore;
using SiteKit.Info;
using System;

#nullable disable

namespace Cohub.Data
{
    public class CohubDbContext : CohubDbContextBase
    {
        [Obsolete("Design-time")]
        public CohubDbContext()
        {
        }

        public CohubDbContext(DbContextOptions<CohubDbContext> options, Actor actor) : base(options, actor)
        {
        }
    }
}
