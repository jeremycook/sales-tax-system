<Query Kind="Statements">
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <NuGetReference>Dapper</NuGetReference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.AnywhereUSA</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>Cohub.Data.Lic</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");

using var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);

// We only care about periods with a due date between fromDate and untilDate
// We only care payment configurations and payment plans that surround this date.
var fromDate = Util.ReadLine("From Date", new DateTime(DateTime.Today.AddDays(-60).Year, DateTime.Today.AddDays(-60).Month, 1));
var untilDate = Util.ReadLine("Until Date", DateTime.Today);

var missingReturns = await db.Set<PaymentConfiguration>()
	.Where(pc => pc.FromDate <= untilDate && untilDate <= (pc.UntilDate ?? DateTime.MaxValue))
	.SelectMany(pc => pc.PaymentChart.Frequency.Periods
		.Where(p => fromDate <= p.DueDate && p.DueDate <= untilDate)
		.SelectMany(p => db.Set<Organization>()
			.Where(o =>
				o.StatusId == OrganizationStatusId.Active &&
				o.PaymentPlans.Any(pp =>
					pp.PaymentChartId == pc.PaymentChartId &&
					pp.FromDate <= untilDate && untilDate <= (pp.UntilDate ?? DateTime.MaxValue) &&
					!o.Returns.Any(r => r.CategoryId == pc.PaymentChart.CategoryId && r.PeriodId == p.Id)
				)
			)
			.Select(o => new Return
			{
				OrganizationId = o.Id,
				CategoryId = pc.PaymentChart.CategoryId,
				PeriodId = p.Id,
				StatusId = p.DueDate >= DateTime.Today ? ReturnStatusId.Shell : ReturnStatusId.Due,
			})
		)
	)
	//.OrderBy(p => p.DueDate).ThenBy(p => p.FromDate)
	.ToArrayAsync();

missingReturns.GroupBy(r => r.CategoryId).Dump();

db.AddRange(missingReturns);
await db.SaveChangesAsync();

//var paymentConfiguration = db.Set<PaymentConfiguration>()
//	.Where(pc => pc.)
