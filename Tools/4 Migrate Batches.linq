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
readonly DateTime minDate = new DateTime(2017, 1, 1);

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var innoDb = this;

	if (reset) // && Util.ReadLine<bool>("Reset?", true))
	{
		db.Database.ExecuteSqlInterpolated($"delete from fin.batch");
	}

	// Lookups
	var userLookup = db.Users().ToDictionary(u => u.LowercaseUsername);

	var batches = innoDb.INNOBATCHes
		.Where(ib => ib.timestamp >= minDate)
		.ToArray();
	var batchGroups = batches
		.GroupBy(ib => ib.BATCHNUMBER.Trim())
		.OrderBy(g => g.Key)
		.ToArray();

	var count = batches.Length;
	var i = 0;
	foreach (var group in batchGroups)
	{
		var groupIndex = group.Count() > 1 ? 0 : -1;
		foreach (var lBatch in group.OrderBy(ib => ib.timestamp.Value))
		{
			var batch = new Batch(lBatch.timestamp.Value, db.GetOrAddUserByUsername(userLookup, lBatch.userstamp).Id)
			{
				Id = i + 1,
				Name = lBatch.BATCHNUMBER + (groupIndex > -1 ? $"-{(char)('A' + groupIndex++)}" : ""),
				DepositControlAmount = lBatch.controlAmount.Value,
				Note = string.Join('\n', new[]
				{
					$"Legacy ID: {((int)lBatch.ID):D8}",
				}),
			};
			var entry = db.Add(batch);
			if (lBatch.postedDate != null)
			{
				entry.Property(o => o.IsPosted).CurrentValue = true;
				entry.Property(o => o.Posted).CurrentValue = lBatch.postedDate.Value;
			}

			i++;
			Util.Progress = (int)(100 * (i / (float)count));
		}
	}
	"Saving changes".Dump();
	db.SaveChanges();
	db.Database.ExecuteSqlRaw($"SELECT setval(pg_get_serial_sequence('fin.batch', 'id'), (select max(id)+1 from fin.batch))");
	"Done".Dump();
}

public static readonly Dictionary<string, string> lTaxTypeToCategoryId = new Dictionary<string, string>
{
	["Lodging"] = "Lodging Tax",
	["Anywhere Public Investment Fund Sales"] = "Anywhere Public Investment Fund Sales Tax",
	["Sales Tax"] = "Sales Tax",
};

public static class Extensions
{
	public static User GetOrAddUserByUsername(this CohubDbContext db, Dictionary<string, User> userLookup, string username)
	{
		var lowerUsername = username.ToLowerInvariant();
		
		if (!userLookup.TryGetValue(lowerUsername, out var user))
		{
			user = db.Users().SingleOrDefault(o => o.LowercaseUsername == lowerUsername);
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
			userLookup.Add(user.LowercaseUsername, user);
		}

		return user;
	}
}