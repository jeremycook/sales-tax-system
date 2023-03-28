using Microsoft.EntityFrameworkCore;
using SiteKit.EntityFrameworkCore;

namespace Cohub.Data.Usr.Configuration
{
    public class UsrModelBuilderContributor : IModelBuilderContributor
    {
        public void ContributeTo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>();
            {
                modelBuilder.Entity<Comment>().HasIndex(o => o.IsUserComment);
            }
            modelBuilder.Entity<Document>();
            modelBuilder.Entity<Role>();
            modelBuilder.Entity<User>();
            {
                modelBuilder.Entity<User>().Property(o => o.RoleId).HasDefaultValueSql("'Disabled'");
                modelBuilder.Entity<User>().Property(o => o.LowercaseUsername).HasComputedColumnSql("lower(username)", stored: true);
                modelBuilder.Entity<User>().HasIndex(o => o.LowercaseUsername).IsUnique();
            }
            modelBuilder.Entity<UserLogin>();
            {
                modelBuilder.Entity<UserLogin>().HasKey(o => new { o.UserId, o.Issuer, o.Sub });
                modelBuilder.Entity<UserLogin>().Property(o => o.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");

            }
            modelBuilder.Entity<UserMention>();
        }
    }
}
