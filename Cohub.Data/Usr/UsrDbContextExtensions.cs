using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Cohub.Data.Usr
{
    public static class UsrDbContextExtensions
    {
        // Comments

        public static DbSet<Comment> Comments(this CohubDbContextBase db)
        {
            return db.Set<Comment>();
        }

        public static IQueryable<Comment> Comments(this CohubDbContextBase db, params int[] ids)
        {
            return db.Comments()
                .IncludeReferences()
                .Where(o => ids.Contains(o.Id))
                .OrderByDescending(o => o.Posted);
        }

        public static IQueryable<Comment> IncludeReferences(this IQueryable<Comment> query)
        {
            return query
                .Include(o => o.Author);
        }

        public static IQueryable<Comment> IncludeCollections(this IQueryable<Comment> query)
        {
            return query
                .Include(o => o.BatchComments)
                .Include(o => o.OrganizationComments)
                .Include(o => o.ReturnComments)
                .Include(o => o.StatementComments)
                .AsSplitQuery();
        }

        // Roles
        
        public static DbSet<Role> Roles(this CohubDbContextBase db)
        {
            return db.Set<Role>();
        }

        // Users

        public static DbSet<User> Users(this CohubDbContextBase db)
        {
            return db.Set<User>();
        }

        public static IQueryable<User> Users(this CohubDbContextBase db, params int[] ids)
        {
            return db.Users()
                .IncludeReferences()
                .IncludeCollections()
                .Where(o => ids.Contains(o.Id))
                .OrderByDescending(o => o.Name);
        }

        public static IQueryable<User> IncludeReferences(this IQueryable<User> query)
        {
            return query
                .Include(o => o.Role)
                .Include(o => o.Locale)
                .Include(o => o.TimeZone);
        }

        public static IQueryable<User> IncludeCollections(this IQueryable<User> query)
        {
            return query
                .Include(o => o.Logins);
        }
    }
}
