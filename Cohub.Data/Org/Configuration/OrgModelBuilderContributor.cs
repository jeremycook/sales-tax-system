using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Org.Configuration
{
    public class OrgModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Label>();
            {
                modelBuilder.Entity<Label>().HasIndex(o => o.GroupId);
                modelBuilder.Entity<Label>().HasIndex(o => o.Title);
            }
            modelBuilder.Entity<Organization>();
            {
                modelBuilder.Entity<Organization>().HasIndex(o => o.StatusId);
                modelBuilder.Entity<Organization>().OwnsOne(o => o.PhysicalAddress);
                modelBuilder.Entity<Organization>().OwnsOne(o => o.MailingAddress);
                modelBuilder.Entity<Organization>()
                    .HasMany(p => p.Labels)
                    .WithMany(p => p.Organizations)
                    .UsingEntity<OrganizationLabel>(
                        j => j.HasOne(pt => pt.Label).WithMany().HasForeignKey(pt => pt.LabelId),
                        j => j.HasOne(pt => pt.Organization).WithMany().HasForeignKey(pt => pt.OrganizationId),
                        j =>
                        {
                            j.Property(pt => pt.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
                            j.HasKey(t => new { t.OrganizationId, t.LabelId });
                        });
            }
            modelBuilder.Entity<OrganizationClassification>();
            modelBuilder.Entity<OrganizationComment>();
            {
                modelBuilder.Entity<OrganizationComment>().HasOne(o => o.Organization).WithMany(o => o.Comments).OnDelete(DeleteBehavior.Cascade);
            }
            modelBuilder.Entity<OrganizationContact>();
            {
                modelBuilder.Entity<OrganizationContact>().OwnsOne(o => o.Address);
            }
            modelBuilder.Entity<OrganizationStatus>();
            modelBuilder.Entity<OrganizationType>();
            modelBuilder.Entity<Relationship>();
        }
    }
}
