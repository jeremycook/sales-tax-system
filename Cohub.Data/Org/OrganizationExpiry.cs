using Cohub.Data.Lic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Org
{
    public class OrganizationExpiry
    {
        private readonly CohubDbContext db;

        public OrganizationExpiry(CohubDbContext db)
        {
            this.db = db;
        }

        public async Task ExpireOrganizationsAsync()
        {
            var threeMonthsAgo = DateTime.Today.AddMonths(-3);

            var expiredOrganizations = await db.Organizations()
                .Where(o =>
                    o.StatusId == OrganizationStatusId.Active &&
                    o.Licenses.Any(l => l.TypeId == LicenseTypeId.Business && threeMonthsAgo > l.ExpirationDate)
                )
                .ToListAsync();

            foreach (var org in expiredOrganizations)
            {
                db.Comment($"Changed organization {org.Id}'s status from {OrganizationStatusId.Active} to {OrganizationStatusId.Expired} because its business license expired more than 3 months ago.", new OrganizationComment(org.Id));
                org.StatusId = OrganizationStatusId.Expired;

                if (org.OnlineFiler)
                {
                    db.Comment($"Disabled organization {org.Id}'s online filer flag.", new OrganizationComment(org.Id));
                    org.OnlineFiler = false;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
