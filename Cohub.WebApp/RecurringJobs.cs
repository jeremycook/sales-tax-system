using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.WebApp
{
    public class RecurringJobs
    {
        private readonly OrganizationExpiry organizationExpiry;
        private readonly ReturnGenerator returnGenerator;
        private readonly ReturnRefresher returnRefresher;

        public static void Schedule(IRecurringJobManager recurringJobManager)
        {
            recurringJobManager.AddOrUpdate<RecurringJobs>("MorningJobs", x => x.MorningJobs(), Cron.Daily(4, 0), TimeZoneInfo.Local);
        }

        public RecurringJobs(OrganizationExpiry organizationExpiry, ReturnGenerator returnGenerator, ReturnRefresher returnRefresher)
        {
            this.organizationExpiry = organizationExpiry;
            this.returnGenerator = returnGenerator;
            this.returnRefresher = returnRefresher;
        }

        [DisableConcurrentExecution(10 * 60)]
        public async Task MorningJobs()
        {
            await organizationExpiry.ExpireOrganizationsAsync();
            await returnGenerator.GenerateMissingReturnsAsync();
            await returnRefresher.RefreshReturnsAsync();
        }
    }
}