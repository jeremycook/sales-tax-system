using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Lic.Configuration
{
    public class LicModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<License>();
            {
                modelBuilder.Entity<License>().HasIndex(o => new { o.TypeId, o.Title }).IsUnique();
            }
            modelBuilder.Entity<LicenseSettings>();
            modelBuilder.Entity<LicenseType>();
        }
    }
}
