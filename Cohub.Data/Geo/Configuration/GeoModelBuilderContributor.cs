using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Geo.Configuration
{
    public class GeoModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Locale>();
            modelBuilder.Entity<State>();
            modelBuilder.Entity<Tz>();
        }
    }
}
