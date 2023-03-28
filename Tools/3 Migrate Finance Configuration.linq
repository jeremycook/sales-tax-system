<Query Kind="Program">
  <Connection>
    <ID>b0f2fd44-0b9a-4b0e-b439-1670b34bb2fc</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost\SQLEXPRESS</Server>
    <Database>anywhereusa_salestaxlive</Database>
    <NoCapitalization>true</NoCapitalization>
    <DriverData>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">C:\Users\jeremy\Repos\anywhereusa\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">C:\Users\jeremy\Repos\anywhereusa\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>LINQPad.Drivers.EFCore</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>Cohub.Data.AnywhereUSA</Namespace>
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");
const bool reset = true;

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var random = new Random();
	var innoDb = this;

	if (reset) // && Util.ReadLine<bool>("Reset?", true))
	{
		db.Database.ExecuteSqlInterpolated($"delete from fin.payment_configuration; delete from fin.period; delete from fin.frequency");
	}

	var lSchedules = (
		from fs in innoDb.FILINGSCHEDULEs
		let fps = FILINGPERIODs.Where(fp => fp.filingSchedule_ID.Value == fs.ID).ToList()
		select new
		{
			FILINGSCHEDULE = fs,
			FILINGPERIODs = fps,
		}
	).ToList();
	var frequencies = lSchedules.Select(fs => new Frequency(fs.FILINGSCHEDULE.timestamp.Value)
	{
		Id = fs.FILINGSCHEDULE.name,
		IsActive = fs.FILINGSCHEDULE.status == "ACTIVE",
		Periods = fs.FILINGPERIODs.GroupBy(fp => fp.year).SelectMany(g => g.OrderBy(fp => fp.period).Select((fp, i) => new Period(fp.timestamp.Value)
		{
			Id =
				fs.FILINGSCHEDULE.name == "Monthly" ? $"{fp.startDate.Value:MMM yyyy}" :
				fs.FILINGSCHEDULE.name == "Quarterly" ? $"Q{i + 1} {fp.startDate.Value:yyyy}" :
				fs.FILINGSCHEDULE.name == "Annually" ? $"Annual {fp.startDate.Value:yyyy}" :
				$"{fp.startDate.Value:yyyy}{fs.FILINGSCHEDULE.name[0]}{(g.Count() >= 10 ? (i + 1).ToString("D2") : (i + 1).ToString("D1"))}",
			Name = fp.description.Trim(),
			DueDate = fp.dueDate.Value,
			StartDate = fp.startDate.Value,
			EndDate = fp.endDate.Value,
			//Description = fp.description,
		})).ToList()
	}).ToList();
	frequencies.Add(new Frequency
	{
		Id = "None",
		IsActive = true,
		Description = null,
		Periods = new List<Period>
		{
			new Period
			{
				Id = PeriodId.None,
				Name = "None",
				StartDate = DateTime.MinValue,
				EndDate = DateTime.MaxValue.AddDays(-1),
				DueDate = DateTime.MaxValue.AddDays(-1),
			}
		}
	});
	db.AddMissingRange(frequencies).Dump(nameof(Frequency));
	db.SaveChanges();

	db.AddMissingRange(frequencies.SelectMany(s => s.Periods)).Dump(nameof(Period));
	db.SaveChanges();

	var returnSchedules = RETSCHEDs
		.Where(rs => rs.filingSchedule_ID != null && rs.vendor.ACCOUNTs.Any(a => a.status == "A"))
		.SelectMany(rs => rs.taxReturn.TAXTYPEs.Select(tt => new
		{
			OrganizationId = rs.vendor.vendorNumber,
			FrequencyId = rs.filingSchedule.name,
			CategoryId = (
				tt.name == "Lodging" ? "Lodging Tax" :
				tt.name == "Sales Tax" ? "Sales Tax" :
				tt.name == "Anywhere Public Investment Fund Sales" ? "Anywhere Public Investment Fund Sales Tax" :
				null
			),
			StartDate = rs.startDate.Value,
			EndDate = rs.endDate,
		}))
		.Where(o => o.OrganizationId != null && o.FrequencyId != null && o.CategoryId != null && o.StartDate != null &&
			(
				(o.OrganizationId.EndsWith("L") && o.CategoryId == "Lodging Tax") ||
				(!o.OrganizationId.EndsWith("L") && o.CategoryId != "Lodging Tax")
			)
		)
		.Distinct()
		.ToArray();

	var lodgingFromDate = returnSchedules.Where(s => s.CategoryId == "Lodging Tax").Min(s => s.StartDate);
	var salesTaxFromDate = returnSchedules.Where(s => s.CategoryId == "Sales Tax").Min(s => s.StartDate);
	var anywherePIFFromDate = returnSchedules.Where(s => s.CategoryId == "Anywhere Public Investment Fund Sales Tax").Min(s => s.StartDate);

	//var paymentConfigurations = returnSchedules
	//	.Select(p => new
	//	{
	//		p.FrequencyId,
	//		p.CategoryId,
	//	})
	//	.Distinct()
	//	.Select(p => new PaymentChart
	//	{
	//		FrequencyId = p.FrequencyId,
	//		CategoryId = p.CategoryId,
	//		IsActive = true,
	//	})
	//	.ToArray();
	//db.AddMissingRange(paymentConfigurations).Dump(nameof(PaymentChart));
	var paymentCharts = new PaymentChart[]
	{
		new PaymentChart { Id = 1, /*Id = "STA",*/ Name = "Annual Sales Tax", IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Annual", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = salesTaxFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3.75m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 2, /*Id = "STM",*/ Name = "Monthly Sales Tax", IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Monthly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = salesTaxFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3.75m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 3, /*Id = "STQ",*/ Name = "Quarterly Sales Tax", IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Quarterly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = salesTaxFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3.75m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },

		new PaymentChart { Id = 4, /*Id = "LTA",*/ Name = "Annual Lodging Tax", IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Annual", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = lodgingFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 5, /*Id = "LTM",*/ Name = "Monthly Lodging Tax", IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Monthly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = lodgingFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 6, /*Id = "LTQ",*/ Name = "Quarterly Lodging Tax", IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Quarterly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = lodgingFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 3m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },

		new PaymentChart { Id = 7, /*Id = "PCSTA",*/ Name = "Annual Prarie Center Sales Tax", IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Annual", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = anywherePIFFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 2.5m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 8, /*Id = "PCSTM",*/ Name = "Monthly Prarie Center Sales Tax", IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Monthly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = anywherePIFFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 2.5m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
		new PaymentChart { Id = 9, /*Id = "PCSTQ",*/ Name = "Quarterly Prarie Center Sales Tax", IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Quarterly", Configurations = new List<PaymentConfiguration> { new PaymentConfiguration { StartDate = anywherePIFFromDate, EndDate = DateTime.MaxValue, MinimumEstimatedNetAmountDue = 600, EstimatedNetAmountDuePercentage = 150, TaxPercentage = 2.5m, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33m } } },
	};
	db.AddMissingRange(paymentCharts).Dump(nameof(PaymentChart));
	db.SaveChanges();
	db.AddMissingRange(paymentCharts.SelectMany(pc => pc.Configurations)).Dump(nameof(PaymentConfiguration));
	db.SaveChanges();

	var maxPaymentChartId = db.PaymentCharts().Max(d => d.Id);
	db.Database.ExecuteSqlRaw($"ALTER TABLE fin.payment_chart ALTER COLUMN id RESTART WITH {maxPaymentChartId}").Dump($"Restarted fin.payment_chart.id with {maxPaymentChartId}");

	if (!db.Set<FilingSchedule>().Any())
	{
		var paymentChartLookup = db.PaymentCharts().ToDictionary(d => d.CategoryId + "+" + d.FrequencyId);

		var paymentPlans = returnSchedules
			.Select(t => new FilingSchedule
			{
				OrganizationId = t.OrganizationId,
				PaymentChartId = paymentChartLookup[t.CategoryId + "+" + t.FrequencyId].Id,
				StartDate = t.StartDate,
				EndDate = t.EndDate ?? DateTime.MaxValue,
			})
			.ToArray();
		paymentPlans.Dump(nameof(FilingSchedule));
		db.AddRange(paymentPlans);
		db.SaveChanges();
	}
}

public static class Extensions
{
	public static User GetOrAddUserByUsername(this CohubDbContext db, Dictionary<string, User> userLookup, string username)
	{
		username = username.ToLower();
		if (!userLookup.TryGetValue(username, out var user))
		{
			user =
				db.Users().Local.SingleOrDefault(o => o.Username == username) ??
				db.Users().SingleOrDefault(o => o.Username == username);
			if (user == null)
			{
				user = new User
				{
					IsActive = false,
					Username = username,
					Name = username,
				};
				db.Add(user);
				db.SaveChanges();
			}
			userLookup.Add(username, user);
		}

		return user;
	}
}