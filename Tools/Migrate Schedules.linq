<Query Kind="Program">
  <Connection>
    <ID>21e05eaf-6033-47d5-aa26-069608c8932c</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.\SQLEXPRESS</Server>
    <Database>anywhereusa_salestaxlive</Database>
    <NoCapitalization>true</NoCapitalization>
    <DriverData>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <EFVersion>5.0.0-preview.5.20278.2</EFVersion>
    </DriverData>
  </Connection>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">&lt;MyDocuments&gt;\Repos\sales-tax-system\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">&lt;MyDocuments&gt;\Repos\sales-tax-system\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

const string connectionString = "Data Source=localhost\\SQLEXPRESS; Initial Catalog=anywhereusa_cohub; Integrated Security=True";

void Main()
{
	using var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseSqlServer(connectionString).Options);
	var innoDb = this;

	//var vendorNumberToAccountId = innoDb.VENDORs.ToDictionary(v => v.vendorNumber, v => v.ID);
	var migratedScheduleIds = db.Schedules.Select(s => s.Id).ToList();

	var filingschedules = innoDb.FILINGSCHEDULEs.Where(fs => !migratedScheduleIds.Contains(fs.name));
	int count = filingschedules.Count().Dump("Schedules");
	int i = 0;
	foreach (var fs in filingschedules.ToArray())
	{
		var schedule = db.Schedules.Find(fs.name);
		if (schedule != null) continue;

		schedule = new Schedule
		{
			Active = fs.status == "ACTIVE",
			Id = fs.name,
			Created = fs.timestamp.Value,
		};
		db.Add(schedule);

		foreach (var fp in fs.FILINGPERIODs.ToArray())
		{
			var period = new Period
			{
				// TODO: Created = per.timestamp,
				Id = $"{fs.name[0]}{fp.year}{fp.period:D2}",
				DueDate = fp.dueDate.Value,
				FirstDate = fp.startDate.Value,
				LastDate = fp.endDate.Value,
				Name = fp.description,
			};
			schedule.Periods.Add(period);
		}

		foreach (var rs in fs.RETSCHEDs.ToArray())
		{
			if (rs.vendor_ID is null)
				continue;

			var invoiceSchedule = new InvoiceSchedule
			{
				Created = rs.timestamp.Value,
				Account = db.GetOrAddAccountByTaxReturn(rs.taxReturn),
				OrganizationId = rs.vendor.vendorNumber,
				FromDate = rs.startDate.Value,
				UntilDate = rs.endDate,
			};
			schedule.InvoiceSchedules.Add(invoiceSchedule);
		}

		if (i % 10 == 0)
		{
			db.SaveChanges();
		}

		i++;
		Util.Progress = (int)(100 * (i / (float)count));
	}
	db.SaveChanges();


}

public static class Extensions
{
	public static Account GetOrAddAccountByTaxReturn(this CohubDbContext db, TAXRETURN tr)
	{
		var account =
			db.Accounts.Local.SingleOrDefault(o => o.Id == tr.name) ??
			db.Accounts.SingleOrDefault(o => o.Id == tr.name);

		if (account == null)
		{
			account = new Account
			{
				Created = tr.timestamp.Value,
				Id = tr.name,
				Active = true,
				Description = null,
			};
			db.Add(account);
		}

		return account;
	}

	public static User GetOrAddUserByUsername(this CohubDbContext db, string username)
	{
		var user =
			db.Users.SingleOrDefault(o => o.Username == username) ??
			db.Users.Local.SingleOrDefault(o => o.Username == username);

		if (user == null)
		{
			user = new User
			{
				Active = true,
				Username = username,
				Name = username,
			};
			db.Add(user);
		}

		return user;
	}
}