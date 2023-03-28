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
  <NuGetReference>Dapper</NuGetReference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.AnywhereUSA</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>Cohub.Data.Lic</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>LINQPad.Drivers.EFCore</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");
const bool reset = true;

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var random = new Random();
	var today = DateTime.Today;
	var threeYearsAgo = today.AddYears(-3);
	var innoDb = this;

	if (reset) // && Util.ReadLine<bool>("Reset?", true))
	{
		db.Database.ExecuteSqlInterpolated($"delete from fin.return");
	}

	"Loading lookups".Dump();
	//var userLookup = db.Users().ToDictionary(u => u.Username);
	//var licenseExpirationLookup = db.Set<License>().GroupBy(l => new { l.Organization.Id, l.Organization.StatusId }).Select(l => new { l.Key.Id, l.Key.StatusId, ExpirationDate = l.Max(lic => lic.ExpirationDate) }).ToDictionary(l => l.Id);
	//var batchLookup = db.Batches()
	//	.Include(o => o.Transactions).ThenInclude(t => t.TransactionDetails)
	//	.ToDictionary(b => (decimal)(int.TryParse(b.Note?.Substring(11, 8), out int val) ? val : random.Next()));
	var periodLookup = db.Set<Period>()
		.AsEnumerable()
		.GroupBy(p => p.StartDate)
		.ToDictionary(g => g.Key, g => g.ToDictionary(p => p.EndDate));
	var paymentPlans = db.Set<FilingSchedule>()
		.Include(pp => pp.PaymentChart).ThenInclude(pc => pc.Frequency).ThenInclude(f => f.Periods)
		.ToList();

	//var payments = db.Set<TransactionDetail>()
	//	.Where(td => td.OperationId == OperationId.CPayment && new[] { Cohub.Data.AnywhereUSA.SubcategoryId.LodgingTax, SubcategoryId.AnywherePIFSalesTax, SubcategoryId.SalesTax }.Contains(td.SubcategoryId))
	//	.AsEnumerable()
	//	.GroupBy(td => new Key(td.OrganizationId, td.SubcategoryId, td.PeriodId))
	//	.Select(g => new { g.Key, Amount = g.Sum(td => td.Amount) })
	//	.ToDictionary(g => g.Key, g => g.Amount);

	//var transactions = (
	//	from td in innoDb.TRANSDETAILs
	//	join v in innoDb.VENDORs on td.ENTITY_ID equals v.ID
	//	select new
	//	{
	//		v.vendorNumber,
	//		td.a
	//	}
	//).ToList();

	//var estimatedReturns = (
	//	from tri in innoDb.TAXRETURNINSTANCEs
	//	join p in innoDb.FILINGPERIODs on tri.period_ID equals p.ID
	//	where tri.estimatedAmountPaid == true
	//	select new
	//	{
	//		OrganizationId = tri.vendor.vendorNumber,
	//		SubcategoryId = db.GetSubcategory(tri.taxReturn.name).Id,
	//		PeriodId = db.GetPeriod(periodLookup, p.description).Id,
	//	}
	//)
	//.AsEnumerable()
	//.GroupBy(g => new Key(g.OrganizationId, g.SubcategoryId, g.PeriodId))
	//.ToDictionary(g => g.Key, g => new { IsTaxDueEstimated = true });

	"Fetching legacy return info".Dump();
	//vendornumber, vendoractive, taxreturnname, filingperioddescription, duedate, isclosed, nettax, excesstax, nettaxable, naicscode, geocode
	var legacyReturnInfo = innoDb.Database.GetDbConnection().QueryAsAnonymousType(() => new
	{
		taxId = 0L,
		triId = 0L,
		vendorNumber = "",
		vendorActive = false,
		taxTypeName = "",
		taxEffectiveDate = DateTime.MinValue,
		dueDate = DateTime.MinValue,
		startDate = DateTime.MinValue,
		endDate = DateTime.MinValue,
		zeroTax = false,
		netTaxable = null as decimal?,
		excessTax = null as decimal?,
		netTax = null as decimal?,
		vendorFee = null as decimal?,
		naicsCode = null as string,
		geocode = null as string,
		amountPaid = null as decimal?,
		amountCharged = null as decimal?,
		balance = null as decimal?,
	}, File.ReadAllText(Path.GetFullPath("./Migrate Returns.sql", Path.GetDirectoryName(Util.CurrentQueryPath)).Dump("SQL")));
	//var paidReportLookup = legacyReturnInfo.ToDictionary(r => new Key(r.vendorNumber, db.GetSubcategory(r.taxTypeName).Id, db.GetPeriod(periodLookup, r.startDate, r.endDate).Id));

	"Generating returns".Dump();
	var returns = new List<Return>();
	foreach (var record in legacyReturnInfo)
	{
		var category = db.GetCategory(record.taxTypeName);
		var ret = new Return(record.taxEffectiveDate)
		{
			CategoryId = category.Id,
			OrganizationId = record.vendorNumber,
			PeriodId = db.GetPeriod(periodLookup, record.startDate, record.endDate).Id,
		};
		returns.Add(ret);

		if (category.TypeId == CategoryTypeId.Tax)
		{
			if (record.zeroTax || record.netTax != null || record.netTaxable != null)
			{
				var filing = new TaxFiling(record.taxEffectiveDate)
				{
					TaxableAmount = record.netTaxable ?? 0,
					ExcessTax = record.excessTax ?? 0,
					FilingDate = record.taxEffectiveDate,
				};
				ret.Filings.Add(filing);
			}
		}
		else if (category.TypeId == CategoryTypeId.Fee)
		{
			if (record.netTax != null)
			{
				var filing = new FeeFiling(record.taxEffectiveDate)
				{
					FeeAmount = record.netTax.Value,
					FilingDate = record.taxEffectiveDate,
				};
				ret.Filings.Add(filing);
			}
		}
		if (record.geocode != null) ret.Labels.Add(db.GetOrAddLabel("Geocode: " + record.geocode, null));
		if (record.naicsCode != null) db.GetOrAddLabel("NAICS: " + record.naicsCode, null);

		if (!record.vendorActive ||
			record.dueDate < threeYearsAgo ||
			record.zeroTax || record.netTax == 0 ||
			((record.netTax - (record.vendorFee ?? 0m) - record.amountPaid) is decimal unpaidAmount && unpaidAmount <= 9.99m))
		{
			ret.StatusId = ReturnStatusId.Closed;
		}
		else if (record.dueDate > DateTime.Today)
		{
			ret.StatusId = ReturnStatusId.Payable;
		}
		else
		{
			ret.StatusId = ReturnStatusId.Due;
		}
	}
	returns.Dump();

	"Adding returns".Dump();
	db.AddRange(returns);

	"Saving changes".Dump();
	db.SaveChanges();
	"Done".Dump();
}

public struct Key
{
	public Key(string organizationId, string categoryId, string periodId)
	{
		OrganizationId = organizationId;
		SubcategoryId = categoryId;
		PeriodId = periodId;
	}

	public override string ToString()
	{
		return $"({OrganizationId},{SubcategoryId},{PeriodId})";
	}

	public string OrganizationId { get; set; }
	public string SubcategoryId { get; set; }
	public string PeriodId { get; set; }
}

public static readonly Dictionary<string, string> transactionDescriptionToCategoryId = new Dictionary<string, string>
{
	["Lodging"] = AnywhereUSACategoryId.LodgingTax,
	["Anywhere Public Investment Fund Sales Tax"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Anywhere Public Investment Fund Sales"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Credit PIF Tax PC"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Credit PIF Tax PIF"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Anywhere Public Investment Fund PIF"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Sales Tax"] = "Sales Tax",
	["Audit: Sales Tax (Interest)"] = AnywhereUSACategoryId.Audit,
	["Audit: Sales Tax (Late Filing Penalty)"] = AnywhereUSACategoryId.Audit,
	["Audit: Sales Tax (Tax)"] = AnywhereUSACategoryId.Audit,
	["Credit PIF Tax PC"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Fee paid by CCR"] = AnywhereUSACategoryId.LicenseFee,
	["Interest"] = AnywhereUSACategoryId.SalesTax,
	["Late Filing Penalty"] = AnywhereUSACategoryId.SalesTax,
	["License Application Fee"] = AnywhereUSACategoryId.LicenseFee,
	["License Application Fee - PAID IN ONESTOP"] = AnywhereUSACategoryId.LicenseFee,
	["License Fee"] = AnywhereUSACategoryId.LicenseFee,
	["License Fee - PAID BY CCR"] = AnywhereUSACategoryId.LicenseFee,
	["License Fee - PAID IN ONE STOP"] = AnywhereUSACategoryId.LicenseFee,
	["License Fee - PAID IN ONESTOP"] = AnywhereUSACategoryId.LicenseFee,
	["License Fee - PAID IN ONETOP"] = AnywhereUSACategoryId.LicenseFee,
	["License Renewal Fee"] = AnywhereUSACategoryId.LicenseFee,
	["Lodging"] = AnywhereUSACategoryId.LodgingTax,
	["NSF Fee"] = AnywhereUSACategoryId.NSFFee,
	["Anywhere Public Investment Fund Sales"] = AnywhereUSACategoryId.AnywherePIFSalesTax,
	["Sales Tax"] = AnywhereUSACategoryId.SalesTax,
	["Special Events"] = AnywhereUSACategoryId.SalesTax,
	["Zero Interest"] = AnywhereUSACategoryId.SalesTax,
	["Zero Penalty"] = AnywhereUSACategoryId.SalesTax,
};

public static class DapperExtensions
{
	[DebuggerStepThrough]
	public static IEnumerable<T> QueryAsAnonymousType<T>(this IDbConnection connection, Func<T> typeBuilder, string sql)
	{
		return connection.Query<T>(sql);
	}
}

public static class Extensions
{
	public static Label GetOrAddLabel(this CohubDbContext db, string labelId, string title)
	{
		var label = db.Set<Label>().Find(labelId);

		if (label == null)
		{
			label = new Label
			{
				Id = labelId,
				IsActive = true,
			};
			if (!string.IsNullOrWhiteSpace(title))
			{
				label.Title = title;
			}
			db.Add(label);
		}

		return label;
	}

	public static Category GetCategory(this CohubDbContext db, string transDetailDescription)
	{
		transactionDescriptionToCategoryId.TryGetValue(transDetailDescription, out var id);
		var entity = db.Set<Category>().Find(id ?? transDetailDescription) ?? throw new ArgumentException($"'{transDetailDescription}' not found");
		return entity;
	}

	public static Period GetPeriod(this CohubDbContext db, Dictionary<DateTime, Dictionary<DateTime, Period>> periodLookup, DateTime startDate, DateTime endDate)
	{
		return periodLookup[startDate][endDate];
	}

	public static Period GetPeriod(this CohubDbContext db, Dictionary<string, Period> periodLookup, string legacyPeriodDescription)
	{
		return periodLookup[legacyPeriodDescription?.Trim() ?? Cohub.Data.Fin.PeriodId.None];
	}

	public static User GetOrAddUserByUsername(this CohubDbContext db, Dictionary<string, User> userLookup, string username)
	{
		var lowerUsername = username.ToLower();
		if (!userLookup.TryGetValue(lowerUsername, out var user))
		{
			user =
				db.Users().Local.SingleOrDefault(o => o.Username == lowerUsername) ??
				db.Users().SingleOrDefault(o => o.Username == lowerUsername);
			if (user == null)
			{
				user = new User
				{
					IsActive = false,
					Username = lowerUsername,
					Name = username,
				};
				db.Add(user);
				db.SaveChanges();
			}
			userLookup.Add(lowerUsername, user);
		}

		return user;
	}
}