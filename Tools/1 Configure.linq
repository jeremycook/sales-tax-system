<Query Kind="Statements">
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">C:\Users\jeremy\Repos\anywhereusa\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">C:\Users\jeremy\Repos\anywhereusa\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <NuGetReference>CsvHelper</NuGetReference>
  <NuGetReference>NodaTime</NuGetReference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.AnywhereUSA</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>CsvHelper</Namespace>
  <Namespace>CsvHelper.Configuration</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>NodaTime.TimeZones</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>SiteKit.Users</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>Humanizer</Namespace>
  <Namespace>Cohub.Data.Lic</Namespace>
  <Namespace>Npgsql</Namespace>
  <RuntimeVersion>5.0</RuntimeVersion>
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");
var csb = new NpgsqlConnectionStringBuilder(connectionString);
var reset = false;

//using var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseSqlServer("Data Source=localhost\\SQLEXPRESS; Initial Catalog=anywhereusa_cohub; Integrated Security=True").Options, Actor.System);
using var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
db.Database.OpenConnection();

if (reset) // && Util.ReadLine<bool>("Reset?", true))
{
	db.Database.ExecuteSqlInterpolated($"delete from usr.user");
}

var http = new HttpClient();

var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
{
	HeaderValidated = null,
	MissingFieldFound = null,
};

{
	NodaTime.IDateTimeZoneProvider provider;
	using (var stream = await http.GetStreamAsync(await http.GetStringAsync("https://nodatime.org/tzdb/latest.txt")))
	{
		var source = TzdbDateTimeZoneSource.FromStream(stream);
		provider = new DateTimeZoneCache(source);
	}
	db.AddMissingRange(provider.Ids.Select(id => new Cohub.Data.Geo.Tz { Id = id })).Dump(nameof(Cohub.Data.Geo.Tz));
	db.SaveChanges();
}

db.AddMissingRange(System.Globalization.CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => c.Name.Count(n => n == '-') == 1).Select(c => new Cohub.Data.Geo.Locale { Id = c.Name })).Dump(nameof(Cohub.Data.Geo.Locale));
db.SaveChanges();

//if (!db.Users().Any())
//{
//	var path = Path.GetFullPath("./usr.user.data.sql", Path.GetDirectoryName(Util.CurrentQueryPath));
//	Util.Cmd("CMD.exe", $@"/C ""C:\Program Files\PostgreSQL\12\bin\psql"" {csb.Database} < {path}".Dump()).Dump();
//	//using var proc = Process.Start($@"""C:\Program Files\PostgreSQL\12\bin\psql""", $@"{csb.Database} < ""{path}""".Dump());
//	//Util.Cmd(@"""C:\Program Files\PostgreSQL\12\bin\pg_restore""", args: $@"-U '{csb.Username}' -d '{csb.Database}' -1 {path}".Dump()).Dump();
//	//var userData = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "./usr.user.data.sql"));
//	//var conn = db.Database.GetDbConnection();
//	//var cmd = conn.CreateCommand();
//	//cmd.CommandText = userData;
//	//cmd.ExecuteNonQuery();
//}

db.AddMissingRange(typeof(RoleId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Role { Id = (string)f.GetValue(null) })).Dump(nameof(RoleId));
db.SaveChanges();

var superRoles = db.Set<Role>().Where(r => r.Id == RoleId.Internal || r.Id == RoleId.Super).ToArray();
var internalRoles = db.Set<Role>().Where(r => r.Id == RoleId.Internal).ToArray();

//db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT usr.[user] ON");
if (db.AddMissingRange(typeof(StandardUserId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new User { Id = (int)f.GetValue(null), Username = f.Name.ToLower(), Name = f.Name, IsActive = false })).ToArray().Dump(nameof(User)).Any())
{
	db.SaveChanges();
}
if (db.AddMissingRange(new[]
{
	new User
	{
		Id = 1000,
		Username = "jcook",
		Name = "Jeremy Cook",
		Initials = "JC",
		IsActive = true,
		LocaleId = "en-US",
		TimeZoneId = "America/Denver",
	},
	new User
	{
		Id = 1001,
		Username = "alescoezec",
		Name = "Ana Lescoezec",
		Initials = "AL",
		IsActive = true,
		LocaleId = "en-US",
		TimeZoneId = "America/Denver",
	},
	new User
	{
		Id = 1002,
		Username = "vshull",
		Name = "Virginia Shull",
		Initials = "VS",
		IsActive = true,
		LocaleId = "en-US",
		TimeZoneId = "America/Denver",
	},
	new User
	{
		Id = 1003,
		Username = "pbreheny",
		Name = "Paul Breheny",
		Initials = "PB",
		IsActive = true,
		LocaleId = "en-US",
		TimeZoneId = "America/Denver",
	},
}).Dump().Any())
{
	db.SaveChanges();
}
//db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT usr.[user] OFF");
var maxUserId = Math.Max(999, db.Users().Max(u => u.Id));
db.Database.ExecuteSqlRaw($"SELECT setval(pg_get_serial_sequence('usr.user', 'id'), (select max(id)+1 from usr.user))");
//db.Database.ExecuteSqlRaw($"ALTER TABLE usr.user ALTER COLUMN id RESTART WITH {maxUserId}").Dump($"Restarted usr.user.id at {maxUserId}");

if (db.AddMissingRange(new[]
{
	new UserRole { UserId = 1000, RoleId = RoleId.Super },
	new UserRole { UserId = 1000, RoleId = RoleId.Internal },

	new UserRole { UserId = 1001, RoleId = RoleId.Super },
	new UserRole { UserId = 1001, RoleId = RoleId.Internal },

	new UserRole { UserId = 1002, RoleId = RoleId.Internal },

	new UserRole { UserId = 1003, RoleId = RoleId.Super },
	new UserRole { UserId = 1003, RoleId = RoleId.Internal },
}).Dump(nameof(UserRole)).Any())
{
	db.SaveChanges();
}

if (db.AddMissingRange(new[]
{
	new UserLogin { UserId = 1000, Issuer = "https://internetid.org", Sub = "cd0f71c7-5e26-40cd-96d8-1b6a1fe75308" },
	new UserLogin { UserId = 1001, Issuer = "https://internetid.org", Sub = "662741b8-f7af-4401-a156-49998c8935a5" },
	new UserLogin { UserId = 1002, Issuer = "https://internetid.org", Sub = "8905379f-4086-4790-9cc6-0279788e2667" },
	new UserLogin { UserId = 1003, Issuer = "https://internetid.org", Sub = "828902e8-0629-42c2-8ae5-e9e433c614b7" },
}).Dump(nameof(UserLogin)).Any())
{
	db.SaveChanges();
}

{
	using var stream = File.Open(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "./NAICSCodes2017.csv"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
	using var reader = new StreamReader(stream);
	using var csv = new CsvReader(reader, csvConfiguration);

	var records = csv.GetRecords<Label>()
		.Where(nc => nc.Value.Length == 4)
		//.Union(new[] { new Label { Id = LabelId.Unknown, Title = "Unknown" } })
		.ToList();
	records.ForEach(nc =>
	{
		nc.IsActive = true;
	});

	db.AddMissingRange(records).Dump(nameof(Label));
	db.SaveChanges();
}

{
	using var stream = File.Open(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "./States.csv"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
	using var reader = new StreamReader(stream);
	using var csv = new CsvReader(reader, csvConfiguration);

	var records = csv.GetRecords<Cohub.Data.Geo.State>()
		.ToList();

	db.AddMissingRange(records).Dump(nameof(Cohub.Data.Geo.State));
	db.SaveChanges();
}

db.AddMissingRange(typeof(BucketId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Bucket { Id = (string)f.GetValue(null), Name = f.Name, IsActive = true })).Dump(nameof(BucketId));
db.SaveChanges();

db.AddMissingRange(typeof(CategoryId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Category { Id = (string)f.GetValue(null), IsActive = true, TypeId = ((string)f.GetValue(null)).Contains("Tax") ? CategoryTypeId.Tax : CategoryTypeId.Fee })).Dump(nameof(CategoryId));
db.SaveChanges();

db.AddMissingRange(typeof(AnywhereUSACategoryId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Category { Id = (string)f.GetValue(null), IsActive = true, TypeId = ((string)f.GetValue(null)).Contains("Tax") ? CategoryTypeId.Tax : CategoryTypeId.Fee })).Dump(nameof(AnywhereUSACategoryId));
db.SaveChanges();

db.AddMissingRange(typeof(SubcategoryId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Subcategory { Id = (string)f.GetValue(null) })).Dump(nameof(SubcategoryId));
db.SaveChanges();

//db.Database.ExecuteSqlInterpolated($"delete from fin.period; delete from fin.schedule");
var numberOfYears = 4;
var startYear = DateTime.Today.Year - numberOfYears + 1;
db.AddMissingRange(new[]
{
	new Frequency
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
	},
	//new Schedule
	//{
	//	Id = "Monthly",
	//	IsActive = true,
	//	Description = null,
	//	Periods = Enumerable.Range(startYear, numberOfYears)
	//		.SelectMany(year => Enumerable.Range(1, 12).Select(month => new DateTime(year, month, 1)))
	//		.Select(date => new Period
	//		{
	//			Id = $"{date:yyyy}M{date:MM}",
	//			Name = $"{date:MMMM} {date:yyyy}",
	//			FromDate = date,
	//			UntilDate = date.AddMonths(1).AddDays(-1),
	//			DueDate = date.AddMonths(1).AddDays(19),
	//		}).ToList()
	//},
	//new Schedule
	//{
	//	Id = "Quarterly",
	//	IsActive = true,
	//	Description = null,
	//	Periods = Enumerable.Range(startYear, numberOfYears)
	//		.SelectMany(year => Enumerable.Range(1, 4).Select(quarter => new
	//		{
	//			Quarter = quarter,
	//			FirstDate = new DateTime(year, 3 * quarter, 1)
	//		}))
	//		.Select(o => new Period
	//		{
	//			Id = $"{o.FirstDate:yyyy}Q{o.Quarter}",
	//			Name = $"Q{o.Quarter} {o.FirstDate:yyyy}",
	//			DueDate = o.FirstDate.AddMonths(3).AddDays(19),
	//			FromDate = o.FirstDate,
	//			UntilDate = o.FirstDate.AddMonths(3).AddDays(-1),
	//		}).ToList()
	//},
	//new Schedule
	//{
	//	Id = "Annual",
	//	IsActive = true,
	//	Description = null,
	//	Periods = Enumerable.Range(startYear, numberOfYears)
	//		.SelectMany(year => Enumerable.Range(1, 1).Select(month => new DateTime(year, month, 1)))
	//		.Select(date => new Period
	//		{
	//			Id = $"{date:yyyy}",
	//			Name = $"Annual {date:yyyy}",
	//			FromDate = date,
	//			UntilDate = date.AddYears(1).AddDays(-1),
	//			DueDate = date.AddYears(1).AddDays(19),
	//		}).ToList()
	//}
}).Dump(nameof(Frequency));
db.SaveChanges();

db.AddMissingRange(typeof(ReturnStatusId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new ReturnStatus { Id = (ReturnStatusId)f.GetValue(null), IsActive = true, Name = f.Name.Titleize() })).Dump(nameof(ReturnStatus));
db.SaveChanges();

//db.AddMissingRange(typeof(ReturnValueKeyId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new ReturnValueKey { Id = (string)f.GetValue(null), Name = ((string)f.GetValue(null)).Titleize() })).Dump(nameof(ReturnValueKey));
//db.SaveChanges();


// Organizations


db.AddMissingRange(typeof(OrganizationStatusId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new OrganizationStatus { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(OrganizationStatus));
db.SaveChanges();

db.AddMissingRange(typeof(OrganizationClassificationId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new OrganizationClassification { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(OrganizationClassification));
db.SaveChanges();

db.AddMissingRange(typeof(RelationshipId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Relationship { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(Relationship));
db.SaveChanges();

db.AddMissingRange(typeof(OrganizationTypeId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new OrganizationType { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(OrganizationType));
db.SaveChanges();

//db.AddMissingRange(typeof(AddressTypeId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new AddressType { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(AddressType));
//db.SaveChanges();
//
//db.AddMissingRange(typeof(IdentificationTypeId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new IdentificationType { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(IdentificationType));
//db.SaveChanges();

db.AddMissingRange(typeof(LicenseTypeId).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new Cohub.Data.Lic.LicenseType { Id = (string)f.GetValue(null), IsActive = true })).Dump(nameof(Cohub.Data.Lic.LicenseType));
db.SaveChanges();

db.AddMissingRange(new[]
{
	new Organization() { Id = "000000", OrganizationName = "Unknown", StatusId = OrganizationStatusId.Archived },
}).Dump(nameof(Organization));
db.SaveChanges();

//db.AddMissingRange(new[]
//{
//	new Organization
//	{
//		Id = "0000001",
//		OrganizationName = "One",
//	},
//}).Dump(nameof(Cohub.Data.Org.Type));
//db.SaveChanges();




//db.AddMissingRange(new[] {
//	new GlType {
//		Id = GlTypeId.Assets,
//		Active = true,
//		GlClasses = {
//			new GlClass {
//				Id = GlClassId.CurrentAssets,
//				Active = true,
//				GlGroups = {
//					new GlGroup { Id = GlGroupId.AuditCash, Active = true, CategoryId = GlCategoryId.Audit },
//					new GlGroup { Id = GlGroupId.LicenseFeeCash, Active = true, CategoryId = GlCategoryId.LicenseFee },
//					new GlGroup { Id = GlGroupId.LodgingTaxCash, Active = true, CategoryId = GlCategoryId.LodgingTax },
//					new GlGroup { Id = GlGroupId.NSFFeeCash, Active = true, CategoryId = GlCategoryId.NSFFee },
//					new GlGroup { Id = GlGroupId.PenaltyCash, Active = true, CategoryId = GlCategoryId.Penalty },
//					new GlGroup { Id = GlGroupId.InterestCash, Active = true, CategoryId = GlCategoryId.Interest },
//					new GlGroup { Id = GlGroupId.AnywherePIFSalesTaxCash, Active = true, CategoryId = GlCategoryId.AnywherePIFSalesTax },
//					new GlGroup { Id = GlGroupId.SalesTaxCash, Active = true, CategoryId = GlCategoryId.SalesTax },
//				}
//			},
//			new GlClass {
//				Id = GlClassId.Receivables,
//				Active = true,
//				GlGroups = {
//					new GlGroup { Id = GlGroupId.AuditReceivables, Active = true, CategoryId = GlCategoryId.Audit },
//					new GlGroup { Id = GlGroupId.LicenseFeeReceivables, Active = true, CategoryId = GlCategoryId.LicenseFee },
//					new GlGroup { Id = GlGroupId.LodgingTaxReceivables, Active = true, CategoryId = GlCategoryId.LodgingTax },
//					new GlGroup { Id = GlGroupId.NSFFeeReceivables, Active = true, CategoryId = GlCategoryId.NSFFee },
//					new GlGroup { Id = GlGroupId.PenaltyReceivables, Active = true, CategoryId = GlCategoryId.Penalty },
//					new GlGroup { Id = GlGroupId.InterestReceivables, Active = true, CategoryId = GlCategoryId.Interest },
//					new GlGroup { Id = GlGroupId.AnywherePIFSalesTaxReceivables, Active = true, CategoryId = GlCategoryId.AnywherePIFSalesTax },
//					new GlGroup { Id = GlGroupId.SalesTaxReceivables, Active = true, CategoryId = GlCategoryId.SalesTax },
//				}
//			}
//		}
//	},
//	new GlType {
//		Id = GlTypeId.Revenue,
//		Active = true,
//		GlClasses = {
//			new GlClass {
//				Id = GlClassId.LicensesAndPermits,
//				Active = true,
//				GlGroups = {
//					new GlGroup { Id = GlGroupId.LicenseFeeRevenue, Active = true, CategoryId = GlCategoryId.LicenseFee },
//				}
//			},
//			new GlClass {
//				Id = GlClassId.Miscellaneous,
//				Active = true,
//				GlGroups = {
//					new GlGroup { Id = GlGroupId.NSFFeeRevenue, Active = true, CategoryId = GlCategoryId.NSFFee },
//					new GlGroup { Id = GlGroupId.PenaltyRevenue, Active = true, CategoryId = GlCategoryId.Penalty },
//					new GlGroup { Id = GlGroupId.InterestRevenue, Active = true, CategoryId = GlCategoryId.Interest },
//				}
//			},
//			new GlClass {
//				Id = GlClassId.Taxes,
//				Active = true,
//				GlGroups = {
//					new GlGroup { Id = GlGroupId.AuditRevenue, Active = true, CategoryId = GlCategoryId.Audit },
//					new GlGroup { Id = GlGroupId.LodgingTaxRevenue, Active = true, CategoryId = GlCategoryId.AnywherePIFSalesTax },
//					new GlGroup { Id = GlGroupId.AnywherePIFSalesTaxRevenue, Active = true, CategoryId = GlCategoryId.SalesTax },
//					new GlGroup { Id = GlGroupId.SalesTaxRevenue, Active = true, CategoryId = GlCategoryId.SalesTax },
//				}
//			},
//		}
//	},
//}).Dump(nameof(GlType));
//db.SaveChanges();
//
//// Gl Accounts
//db.AddMissingRange(new[] {
//	new GlAccount { Id = "10-00-0000-01000", Description = "Cash-Combined Fund", ClassId = GlClassId.CurrentAssets },
//	new GlAccount { Id = "10-11-3100-03004", Description = "SALES TAX General", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "10-11-3100-03011", Description = "SALES TAX - RECREATION", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "10-11-3100-03545", Description = "Sales Tax Audit Revenue", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "10-11-3200-03031", Description = "SALES TAX LICENSE", ClassId = GlClassId.LicensesAndPermits },
//	new GlAccount { Id = "10-12-4150-03544", Description = "Interest & Penalties - Collections and Late Filing", ClassId = GlClassId.Miscellaneous },
//	new GlAccount { Id = "27-00-0000-01000", Description = "Cash-Combined Fund", ClassId = GlClassId.CurrentAssets },
//	new GlAccount { Id = "27-11-3100-03523", Description = "Sales tax - Lodging", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "30-00-0000-01000", Description = "Cash-Combined Fund", ClassId = GlClassId.CurrentAssets },
//	new GlAccount { Id = "30-11-3100-03004", Description = "SALES TAX Cap Imp", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "30-11-3100-03545", Description = "Sales Tax Audit Revenue", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "32-00-0000-01000", Description = "Cash-Combined Fund", ClassId = GlClassId.CurrentAssets },
//	new GlAccount { Id = "32-11-3100-03004", Description = "SALES TAX Recr Cap", ClassId = GlClassId.Taxes },
//	new GlAccount { Id = "32-11-3100-03545", Description = "Sales Tax Audit Revenue", ClassId = GlClassId.Taxes },
//}).Dump(nameof(GlAccount));
//db.SaveChanges();
//
//var beginningOfTime = new DateTime(2000, 1, 1);
//if (!db.GlAccountAllocations.Any()) db.AddRange(new[] {
//    // Lodging Tax
//    new GlAccountAllocation { GlAccountId = "27-11-3100-03523", GlGroupId = GlGroupId.LodgingTaxRevenue, FirstDate = beginningOfTime, Percent = 100 },
//	new GlAccountAllocation { GlAccountId = "27-00-0000-01000", GlGroupId = GlGroupId.LodgingTaxCash, FirstDate = beginningOfTime, Percent = 100 },
//    // Sales Tax
//    new GlAccountAllocation { GlAccountId = "10-11-3100-03011", GlGroupId = GlGroupId.SalesTaxRevenue, FirstDate = beginningOfTime, Percent = 04 },
//	new GlAccountAllocation { GlAccountId = "30-11-3100-03004", GlGroupId = GlGroupId.SalesTaxRevenue, FirstDate = beginningOfTime, Percent = 26.7 },
//	new GlAccountAllocation { GlAccountId = "10-11-3100-03004", GlGroupId = GlGroupId.SalesTaxRevenue, FirstDate = beginningOfTime, Percent = 56 },
//	new GlAccountAllocation { GlAccountId = "32-11-3100-03004", GlGroupId = GlGroupId.SalesTaxRevenue, FirstDate = beginningOfTime, Percent = 13.3 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.SalesTaxCash, FirstDate = beginningOfTime, Percent = 60 },
//	new GlAccountAllocation { GlAccountId = "30-00-0000-01000", GlGroupId = GlGroupId.SalesTaxCash, FirstDate = beginningOfTime, Percent = 26.7 },
//	new GlAccountAllocation { GlAccountId = "32-00-0000-01000", GlGroupId = GlGroupId.SalesTaxCash, FirstDate = beginningOfTime, Percent = 13.3 },
//    // Anywhere Public Investment Fund Sales Tax
//    new GlAccountAllocation { GlAccountId = "10-11-3100-03004", GlGroupId = GlGroupId.AnywherePIFSalesTaxRevenue, FirstDate = beginningOfTime, Percent = 84 },
//	new GlAccountAllocation { GlAccountId = "32-11-3100-03004", GlGroupId = GlGroupId.AnywherePIFSalesTaxRevenue, FirstDate = beginningOfTime, Percent = 10 },
//	new GlAccountAllocation { GlAccountId = "10-11-3100-03011", GlGroupId = GlGroupId.AnywherePIFSalesTaxRevenue, FirstDate = beginningOfTime, Percent = 6 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.AnywherePIFSalesTaxCash, FirstDate = beginningOfTime, Percent = 90 },
//	new GlAccountAllocation { GlAccountId = "32-00-0000-01000", GlGroupId = GlGroupId.AnywherePIFSalesTaxCash, FirstDate = beginningOfTime, Percent = 10 },
//    // Audit Revenue
//    new GlAccountAllocation { GlAccountId = "10-11-3100-03545", GlGroupId = GlGroupId.AuditRevenue, FirstDate = beginningOfTime, Percent = 60 },
//	new GlAccountAllocation { GlAccountId = "30-11-3100-03545", GlGroupId = GlGroupId.AuditRevenue, FirstDate = beginningOfTime, Percent = 26.7 },
//	new GlAccountAllocation { GlAccountId = "32-11-3100-03545", GlGroupId = GlGroupId.AuditRevenue, FirstDate = beginningOfTime, Percent = 13.3 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.AuditCash, FirstDate = beginningOfTime, Percent = 60 },
//	new GlAccountAllocation { GlAccountId = "30-00-0000-01000", GlGroupId = GlGroupId.AuditCash, FirstDate = beginningOfTime, Percent = 26.7 },
//	new GlAccountAllocation { GlAccountId = "32-00-0000-01000", GlGroupId = GlGroupId.AuditCash, FirstDate = beginningOfTime, Percent = 13.3 },
//    // License Fee
//    new GlAccountAllocation { GlAccountId = "10-11-3200-03031", GlGroupId = GlGroupId.LicenseFeeRevenue, FirstDate = beginningOfTime, Percent = 100 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.LicenseFeeCash, FirstDate = beginningOfTime, Percent = 100 },
//    // Penalty
//    new GlAccountAllocation { GlAccountId = "10-12-4150-03544", GlGroupId = GlGroupId.PenaltyRevenue, FirstDate = beginningOfTime, Percent = 100 },
//	new GlAccountAllocation { GlAccountId = "10-12-4150-03544", GlGroupId = GlGroupId.PenaltyCash, FirstDate = beginningOfTime, Percent = 100 },
//    // Interest
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.InterestRevenue, FirstDate = beginningOfTime, Percent = 100 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.InterestCash, FirstDate = beginningOfTime, Percent = 100 },
//    // NSF Fee
//    new GlAccountAllocation { GlAccountId = "10-12-4150-03544", GlGroupId = GlGroupId.NSFFeeRevenue, FirstDate = beginningOfTime, Percent = 100 },
//	new GlAccountAllocation { GlAccountId = "10-00-0000-01000", GlGroupId = GlGroupId.NSFFeeCash, FirstDate = beginningOfTime, Percent = 100 },
//}.Dump(nameof(GlAccountAllocation)));
//db.SaveChanges();
