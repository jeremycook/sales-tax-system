using Cohub.Data.Fin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.Data.Org
{
    public static class OrgDbContextExtensions
    {
        private static int minNext = 1;

        public static DbSet<Label> Labels(this CohubDbContextBase db)
        {
            return db.Set<Label>();
        }

        public static IQueryable<Label> Labels(this CohubDbContextBase db, params string[] ids)
        {
            return db.Labels()
                .Where(o => ids.Contains(o.Id))
                .OrderBy(o => o.Id);
        }

        public static DbSet<Organization> Organizations(this CohubDbContextBase db)
        {
            return db.Set<Organization>();
        }

        public static IQueryable<Organization> Organizations(this CohubDbContextBase db, params string[] ids)
        {
            return db.Organizations()
                .AsSplitQuery()
                .IncludeReferences()
                .IncludeLabels()
                .Where(o => ids.Contains(o.Id))
                .OrderBy(o => o.Id);
        }

        /// <summary>
        /// Includes references.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Organization> IncludeReferences(this IQueryable<Organization> query)
        {
            return query
                .Include(o => o.Status)
                .Include(o => o.Classification)
                .Include(o => o.Type)
                .Include(o => o.CreatedBy)
                .Include(o => o.UpdatedBy);
        }

        /// <summary>
        /// Includes low volume child collections.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<Organization> IncludeCollections(this IQueryable<Organization> query)
        {
            return query
                .AsSplitQuery()
                .Include(o => o.Licenses).ThenInclude(o => o.Type)
                .Include(o => o.Labels)
                .Include(o => o.Contacts).ThenInclude(o => o.Relationship)
                .Include(o => o.FilingSchedules).ThenInclude(o => o.PaymentChart);
        }

        [Obsolete("Exceptions at runtime. Use Organization.LoadCommentsAsync until this is fixed.", error: true)]
        public static IQueryable<Organization> IncludeComments(this IQueryable<Organization> query, int take)
        {
            return query
                .Include(o => o.Comments.OrderByDescending(oc => oc.Comment.Posted).Take(take))
                .Include(o => o.Comments).ThenInclude(oc => oc.Comment).ThenInclude(c => c.Author);
        }

        public static IQueryable<Organization> IncludeOpenReturns(this IQueryable<Organization> query)
        {
            return query
                .Include(o => o.Returns.Where(o => ReturnStatus.OpenIds.Contains(o.StatusId)))
                .Include(o => o.Returns).ThenInclude(o => o.Status)
                .Include(o => o.Returns).ThenInclude(o => o.Period)
                .Include(o => o.Returns).ThenInclude(o => o.Category);
        }

        public static IQueryable<Organization> IncludeLabels(this IQueryable<Organization> query)
        {
            return query
                .Include(o => o.Labels.OrderBy(o => o.Id));
        }

        /// <summary>
        /// Calculates the next Organization Id. Note that two people could end up with the same Id if creating organizations at the same time.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<string> NextIdAsync(this DbSet<Organization> query)
        {
            string latest = await query
                .Select(o => o.Id)
                .OrderBy(o => o)
                .LastOrDefaultAsync();

            latest = query.Local
                .Select(o => o.Id).Append(latest)
                .OrderBy(o => o)
                .LastOrDefault();

            int next;
            if (!string.IsNullOrWhiteSpace(latest) && Regex.Match(latest, @"\d+") is Match match && match.Success)
            {
                next = int.Parse(match.Value) + 1;
                if (next < minNext)
                {
                    next = minNext;
                }
            }
            else
            {
                next = minNext;
            }

            minNext = next + 1;

            string nextId = $"{next:D6}";
            return nextId;
        }

        public static DbSet<OrganizationContact> OrganizationContacts(this CohubDbContextBase db)
        {
            return db.Set<OrganizationContact>();
        }

        public static IQueryable<OrganizationContact> OrganizationContacts(this CohubDbContextBase db, params int[] ids)
        {
            return db.OrganizationContacts()
                .IncludeReferences()
                .Where(o => ids.Contains(o.Id))
                .OrderBy(o => o.Id);
        }

        public static IQueryable<OrganizationContact> IncludeReferences(this IQueryable<OrganizationContact> query)
        {
            return query
                .Include(o => o.Organization)
                .Include(o => o.Relationship);
        }

        public static DbSet<OrganizationType> OrganizationTypes(this CohubDbContextBase db)
        {
            return db.Set<OrganizationType>();
        }
    }
}
