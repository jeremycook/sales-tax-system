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
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>SiteKit.Info</Namespace>
  <Namespace>SiteKit.Users</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>Cohub.Data.Lic</Namespace>
  <Namespace>Cohub.Data.Geo</Namespace>
  <Namespace>SiteKit.Text</Namespace>
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");
const bool reset = true;

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var innoDb = this;

	if (reset)
	{
		db.Database.ExecuteSqlInterpolated($"delete from org.organization_comment");
	}
	var userLookup = db.Users().ToDictionary(u => u.Username);

	// Mapping from account number to legacy account ID
	var accountNumberToAccountId = innoDb.ACCOUNTs.ToDictionary(a => a.vendor.vendorNumber, a => a.ID);

	var accountNotes = innoDb.ACCOUNT_NOTEs
		.AsEnumerable()
		.GroupBy(an => (long)an.Account_ID)
		.ToDictionary(an => an.Key, g => g.Select(an => new { an.notes.note, timestamp = an.notes.timestamp.Value, an.notes.userstamp }).ToArray());

	// Migrate organization notes
	var orgs = db.Organizations()
		.Where(o => !o.Comments.Any())
		.ToList();
	var count = orgs.Count.Dump("Migrating Organization Comments");
	var i = 0;
	foreach (var org in orgs)
	{
		if (!accountNotes.TryGetValue(accountNumberToAccountId[org.Id], out var notes))
		{
			continue;
		}
		foreach (var note in notes.OrderByDescending(n => n.timestamp))
		{
			var comment = new Comment(new OrganizationComment(org))
			{
				Text = note.note,
				Html = Html.Sanitize(note.note).ToString(),
				Author = db.GetOrAddUserByUsername(userLookup, note.userstamp),
				Posted = note.timestamp,
			};
			db.Add(comment);
		}

		//if (i % 10 == 0)
		//{
		//	db.SaveChanges();
		//}

		i++;
		Util.Progress = (int)(100 * (i / (float)count));
	}
	db.SaveChanges();

	foreach (var org in orgs)
	{
		db.Comment("Migrated organization.", new OrganizationComment(org));
	}
	db.SaveChanges();
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