using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Lic
{
    public static class LicDbContextExtensions
    {
        public static DbSet<License> Licenses(this CohubDbContext db)
        {
            return db.Set<License>();
        }

        public static IQueryable<License> IncludeReferences(this IQueryable<License> query)
        {
            return query
                .Include(o => o.Organization)
                .Include(o => o.Type);
        }

        public static IQueryable<License> Licenses(this CohubDbContext db, params int[] ids)
        {
            return db.Licenses()
                .IncludeReferences()
                .Where(o => ids.Contains(o.Id))
                .OrderBy(o => o.Title);
        }

        /// <summary>
        /// Returns the current <see cref="LicenseSettings"/>.
        /// If one does not exist then a default will be created and persisted.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<LicenseSettings> LicenseSettingsAsync(this CohubDbContext db)
        {
            var settings = await db.Set<LicenseSettings>()
                .OrderBy(o => o.Id)
                .LastOrDefaultAsync();

            if (settings is null)
            {
                settings = new()
                {
                    CurrentBusinessLicenseExpirationDate = new DateTime(DateTime.Today.Year, 12, 31),
                    NextBusinessLicenseExpirationDate = new DateTime(DateTime.Today.Year + 2, 12, 31),
                };
                db.Add(settings);
                await db.SaveChangesAsync();
            }

            return settings;
        }
    }
}
