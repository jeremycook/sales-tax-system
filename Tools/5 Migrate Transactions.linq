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
  <Namespace>Cohub.Data.Lic</Namespace>
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
		//db.Database.ExecuteSqlInterpolated($"delete from fin.payment_plan");
		db.Database.ExecuteSqlInterpolated($"delete from fin.transaction_detail");
		db.Database.ExecuteSqlInterpolated($"delete from fin.transaction");
	}

	"Loading lookups".Dump();
	var userLookup = db.Users().ToDictionary(u => u.Username);
	var licenseExpirationLookup = db.Set<License>().GroupBy(l => new { l.Organization.Id, l.Organization.StatusId }).Select(l => new { l.Key.Id, l.Key.StatusId, ExpirationDate = l.Max(lic => lic.ExpirationDate) }).ToDictionary(l => l.Id);
	var batchLookup = db.Batches()
		.Include(o => o.Transactions).ThenInclude(t => t.Details)
		.ToDictionary(b => (decimal)(int.TryParse(b.Note?.Substring(11, 8), out int val) ? val : random.Next()));
	var periodLookup = db.Set<Period>().ToDictionary(p => p.Name);

	var legacyPeriods = innoDb.FILINGPERIODs.ToDictionary(fp => fp.ID);

	//"Adding payment configuration".Dump();
	//
	//db.AddMissingRange(new PaymentConfiguration[]
	//{
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Annual", EstimatedTaxDue = 600, TaxPercentage = 3f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Monthly", EstimatedTaxDue = 600, TaxPercentage = 3f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Lodging Tax", FrequencyId = "Quarterly", EstimatedTaxDue = 600, TaxPercentage = 3f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Annual", EstimatedTaxDue = 600, TaxPercentage = 2.25f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Monthly", EstimatedTaxDue = 600, TaxPercentage = 2.25f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Anywhere Public Investment Fund Sales Tax", FrequencyId = "Quarterly", EstimatedTaxDue = 600, TaxPercentage = 2.25f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Annual", EstimatedTaxDue = 600, TaxPercentage = 3.75f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Monthly", EstimatedTaxDue = 600, TaxPercentage = 3.75f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//	new PaymentConfiguration { IsActive = true, CategoryId = "Sales Tax", FrequencyId = "Quarterly", EstimatedTaxDue = 600, TaxPercentage = 3.75f, PenaltyPercentage = 10, InterestPercentage = 1, VendorFeeMax = 25, VendorFeePercentage = 3.33f },
	//}).Dump(nameof(PaymentConfiguration));
	//db.SaveChanges();

	"Fetching legacy transactions".Dump();

	var query =
		from ib in INNOBATCHes.Where(innobatch => batchLookup.Keys.Contains(innobatch.ID))
		join td in TRANSDETAILs on ib.ID equals td.batch_ID
		join v in VENDORs on td.ENTITY_ID equals v.ID
		let tri = TAXRETURNINSTANCEs.FirstOrDefault(x => x.ID == (long)td.mainSource_ID)
		let p = FILINGPERIODs.FirstOrDefault(x => x.ID == tri.period_ID)
		select new
		{
			td.batch_ID,
			ib.timestamp,
			ib.userstamp,
			ib.controlAmount,
			ib.lockboxFilename,
			ib.postedDate,
			ib.batchType,
			td.transactionId,
			td.captureDate,
			effectiveDate = td.effectiveDate,
			td.id,
			vendor_ID = v.ID,
			v.vendorNumber,
			td.transactionType,
			td.description,
			td.amount,
			periodDescription = p.description
		};

	var transdetails = query
		.OrderBy(q => q.id)
		.ToArray();

	//if (!db.Set<FilingSchedule>().Any())
	//{
	//	"Fetching legacy return schedules".Dump();
	//
	//	var returnSchedules = RETSCHEDs
	//		.Where(rs => rs.filingSchedule_ID != null && rs.vendor.ACCOUNTs.Any(a => a.status == "A"))
	//		.SelectMany(rs => rs.taxReturn.TAXTYPEs.Select(tt => new
	//		{
	//			OrganizationId = rs.vendor.vendorNumber,
	//			FrequencyId = rs.filingSchedule.name,
	//			CategoryId = (
	//				tt.name == "Lodging" ? "Lodging Tax" :
	//				tt.name == "Sales Tax" ? "Sales Tax" :
	//				tt.name == "Anywhere Public Investment Fund Sales" ? "Anywhere Public Investment Fund Sales Tax" :
	//				null
	//			),
	//			FromDate = rs.startDate.Value,
	//			UntilDate = rs.endDate,
	//		}))
	//		.Where(o => o.OrganizationId != null && o.FrequencyId != null && o.CategoryId != null && o.FromDate != null)
	//		.Distinct()
	//		.ToArray();
	//
	//	var organizationCategories = transdetails
	//		.Select(td => new
	//		{
	//			td.vendorNumber,
	//			td.description
	//		})
	//		.Distinct()
	//		.AsEnumerable()
	//		.Select(td => new
	//		{
	//			OrganizationId = td.vendorNumber,
	//			CategoryId = db.GetCategory(td.description).Id,
	//		})
	//		.Distinct()
	//		.ToDictionary(o => o);
	//
	//	var paymentPlans = returnSchedules
	//		.Where(rs => organizationCategories.ContainsKey(new { rs.OrganizationId, rs.CategoryId }))
	//		.Select(t => new FilingSchedule
	//		{
	//			OrganizationId = t.OrganizationId,
	//			PaymentConfigurationId = t.FrequencyId.Substring(0, 2) + ":" + t.CategoryId,
	//			FromDate = t.FromDate,
	//			UntilDate = t.UntilDate,
	//		})
	//		.ToArray();
	//	paymentPlans.Dump(nameof(FilingSchedule));
	//	db.AddRange(paymentPlans);
	//	db.SaveChanges();
	//}

	"Adding transactions and details".Dump();

	var batchGroups = transdetails
		.GroupBy(td => new { td.batch_ID, td.transactionId, td.timestamp, td.effectiveDate })
		.OrderBy(td => td.Key.batch_ID);

	var count = transdetails.Length;
	var i = 0;
	foreach (var ltransaction in batchGroups)
	{
		var batch = batchLookup[(int)ltransaction.Key.batch_ID];

		var transaction = new Cohub.Data.Fin.Transaction(ltransaction.Key.timestamp.Value)
		{
			Note = $"Legacy ID: {ltransaction.Key.transactionId}",
		};
		batch.Transactions.Add(transaction);
		db.Entry(transaction).Collection(o => o.Details).CurrentValue = new List<TransactionDetail>();

		foreach (var ldetail in ltransaction.OrderBy(ld => ld.id))
		{
			var period = db.GetPeriod(periodLookup, ldetail.periodDescription ?? "None");

			var detail = Info.Create(ldetail.timestamp.Value, ldetail.description, ldetail.transactionType, ldetail.amount ?? 0);
			detail.OrganizationId = ldetail.vendorNumber;
			detail.PeriodId = period.Id;
			detail.EffectiveDate = ltransaction.Key.effectiveDate.Value;
			detail.Note = $"Legacy transaction type and description: {ldetail.transactionType}, {ldetail.description}";
			transaction.Details.Add(detail);

			i++;
			Util.Progress = (int)(100 * (i / (float)count));
		}

		//db.Entry(batch).State = EntityState.Modified;
		//db.Entry(batch).Property(b => b.Debited).IsModified = true;
		//db.Entry(batch).Property(b => b.Credited).IsModified = true;
	}

	"Saving changes".Dump();
	db.SaveChanges();
	"Done".Dump();
}

public class Info
{
	public string CategoryId { get; set; }
	public string SubcategoryId { get; set; }

	public static TransactionDetail Create(DateTimeOffset created, string legacyTransactionDescription, string legacyTransactionType, decimal legacyAmount)
	{
		var info = transactionDescriptionToInfo[legacyTransactionDescription];

		var td = new TransactionDetail(created)
		{
			CategoryId = info.CategoryId,
			SubcategoryId = info.SubcategoryId,
		};

		//["CHARGE"] = BucketId.Revenue,
		//["NSF"] = BucketId.Revenue,
		//["CREDIT_USED"] = BucketId.Payable, // -Payable
		//["REFUND"] = BucketId.Payable, // -Payable
		//
		//["NSF_PAYMENT"] = BucketId.Deposit,
		//["PAYMENT"] = BucketId.Deposit,
		//
		//["OVERPAYMENT"] = BucketId.Payable,
		//["CREDIT_APPLY"] = BucketId.Payable,
		//["WRITE_OFF"] = BucketId.Adjustment,
		if (false)
		{
		}
		//else if (legacyTransactionType == "CREDIT_USED")
		//{
		//	td.BucketId = Cohub.Data.Fin.BucketId.Payable;
		//	td.Amount = -legacyAmount;
		//}
		//else if (legacyTransactionType == "REFUND")
		//{
		//	td.BucketId = Cohub.Data.Fin.BucketId.Payable;
		//	td.Amount = -legacyAmount;
		//}
		//else if (legacyTransactionType == "NSF")
		//{
		//	td.BucketId = Cohub.Data.Fin.BucketId.Revenue;
		//	td.Amount = legacyAmount;
		//}
		else
		{
			td.BucketId = transactionTypeToBucketId[legacyTransactionType];
			td.Amount = legacyAmount;
		}

		return td;
	}
}

public static readonly Dictionary<string, Info> transactionDescriptionToInfo = new Dictionary<string, Info>
{
	["Lodging"] = new Info { CategoryId = AnywhereUSACategoryId.LodgingTax, SubcategoryId = SubcategoryId.Net },
	["Anywhere Public Investment Fund Sales Tax"] = new Info { CategoryId = AnywhereUSACategoryId.AnywherePIFSalesTax, SubcategoryId = SubcategoryId.Net },
	["Anywhere Public Investment Fund Sales"] = new Info { CategoryId = AnywhereUSACategoryId.AnywherePIFSalesTax, SubcategoryId = SubcategoryId.Net },
	//["Credit PIF Tax PC"] = new Info { StreamId = AnywhereUSACategoryId.AnywherePIFSalesTax, CategoryId = SubcategoryId.Net },
	["Sales Tax"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Net },
	["Audit: Sales Tax (Interest)"] = new Info { CategoryId = AnywhereUSACategoryId.Audit, SubcategoryId = SubcategoryId.Interest },
	["Audit: Sales Tax (Late Filing Penalty)"] = new Info { CategoryId = AnywhereUSACategoryId.Audit, SubcategoryId = SubcategoryId.Penalty },
	["Audit: Sales Tax (Tax)"] = new Info { CategoryId = AnywhereUSACategoryId.Audit, SubcategoryId = SubcategoryId.Net },
	//["Credit PIF Tax PC"] = new Info { StreamId = AnywhereUSACategoryId.AnywherePIFSalesTax, CategoryId = SubcategoryId.Net },
	["Fee paid by CCR"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["Interest"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Interest },
	["Late Filing Penalty"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Penalty },
	["License Application Fee"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Application Fee - PAID IN ONESTOP"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Fee"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Fee - PAID BY CCR"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Fee - PAID IN ONE STOP"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Fee - PAID IN ONESTOP"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Fee - PAID IN ONETOP"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["License Renewal Fee"] = new Info { CategoryId = AnywhereUSACategoryId.LicenseFee, SubcategoryId = SubcategoryId.Net },
	["Lodging"] = new Info { CategoryId = AnywhereUSACategoryId.LodgingTax, SubcategoryId = SubcategoryId.Net },
	["NSF Fee"] = new Info { CategoryId = AnywhereUSACategoryId.NSFFee, SubcategoryId = SubcategoryId.Net },
	["Anywhere Public Investment Fund Sales"] = new Info { CategoryId = AnywhereUSACategoryId.AnywherePIFSalesTax, SubcategoryId = SubcategoryId.Net },
	["Sales Tax"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Net },
	["Special Events"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Net },
	["Zero Interest"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Penalty },
	["Zero Penalty"] = new Info { CategoryId = AnywhereUSACategoryId.SalesTax, SubcategoryId = SubcategoryId.Interest },
};

public static readonly Dictionary<string, string> transactionTypeToBucketId = new Dictionary<string, string>
{
	["NSF_PAYMENT"] = BucketId.Deposit,
	["PAYMENT"] = BucketId.Deposit,

	["CHARGE"] = BucketId.Revenue,

	["NSF"] = BucketId.Overpayment,
	["CREDIT_USED"] = BucketId.Overpayment,
	["REFUND"] = BucketId.Overpayment,
	["OVERPAYMENT"] = BucketId.Overpayment,
	["CREDIT_APPLY"] = BucketId.Overpayment,
	
	["WRITE_OFF"] = BucketId.Adjustment,
};

public static class Extensions
{
	public static Bucket GetBucket(this CohubDbContext db, string lTransactionType)
	{
		var id = transactionTypeToBucketId[lTransactionType];
		var entity = db.Set<Bucket>().Find(id) ?? throw new ArgumentException($"'{lTransactionType}' not found");
		return entity;
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