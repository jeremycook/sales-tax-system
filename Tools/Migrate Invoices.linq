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
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\Cohub.Data.dll</Reference>
  <Reference Relative="..\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll">&lt;MyDocuments&gt;\Repos\AnywhereUSA\Cohub.Data\bin\Debug\netstandard2.1\SiteKit.dll</Reference>
  <Namespace>Cohub.Data</Namespace>
  <Namespace>Cohub.Data.Org</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>Cohub.Data.Usr</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Cohub.Data.Fin</Namespace>
  <Namespace>LINQPad.Drivers.EFCore</Namespace>
  <Namespace>SiteKit.Info</Namespace>
</Query>

//const string connectionString = "Data Source=localhost\\SQLEXPRESS; Initial Catalog=anywhereusa_cohub; Integrated Security=True";
string connectionString = "Server=127.0.0.1;Port=5432;Database=anywhereusa_cohub2;User Id=jeremy_csc;Password=hsgyzaftqjdimhx4xczgw5uf2;Include Error Detail=true";

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var innoDb = this;

	//var vendorNumberToAccountId = innoDb.VENDORs.ToDictionary(v => v.vendorNumber, v => v.ID);
	//var migratedScheduleIds = db.Schedules.Select(s => s.Id).ToList();

	var query =
		from td in innoDb.TRANSDETAILs
		join tri in innoDb.TAXRETURNINSTANCEs on td.mainSource_ID equals tri.ID
		//join ts in innoDb.TRANSSOURCEs on tri.ID equals ts.ID
		join fp in innoDb.FILINGPERIODs on tri.period_ID equals fp.ID
		//group  new { tri.vendor } into grouping
		select new
		{
			transdetails_id = td.id,
			td.batch_ID,
			tri.vendor.vendorNumber,
			period = fp,
			schedule = fp.filingSchedule,
			tri.dueDate,
			tri.startDate,
			tri.endDate,
			tri.filingDate,
			td_description = td.description,
			td.transactionType,
			td.captureDate,
			td.effectiveDate,
			td.postedDate,
			td.batch.BATCHNUMBER,
			td.transactionId,
			td.multiplier,
			is_payment = td.multiplier >= 0 ? true : false,
			td.amount,
			signed_amount = td.multiplier < 0 ? -td.amount : td.amount
		};

	var result = query.ToList();

	//from tri in innoDb.TAXRETURNINSTANCEs
	//join tr in innoDb.TAXRETURNs on tri.taxReturn_ID equals tr.ID
	////let td = innoDb.TRANSDETAILs.Where(x => x.mainSource_ID == tri.ID)
	//let paid = innoDb.TRANSDETAILs.Where(x => x.mainSource_ID == tri.ID && x.transactionType == "PAYMENT").Sum(x => x.multiplier * x.amount)
	//let penalty = innoDb.TRANSSOURCEs.Where(x => x.ID == tri.ID).Sum(x => x.penaltyInstance.penaltyAmount)
	//let interest = innoDb.TRANSSOURCEs.Where(x => x.ID == tri.ID).Sum(x => x.interestInstance.interestAmount)
	////let tax = paid - penalty - interest
	////let tri_notes = innoDb.TAXRETURNINSTANCE_NOTEs.Where(x => x.TaxReturnInstance_ID == tri.ID).Select(x => x.notes)
	//select new
	//{
	//	tri,
	//	//tr,
	//	paid,
	//	penalty,
	//	interest,
	//	//tax,
	//	//td,
	//	//tri_notes,
	//};

	var records = query.ToList();
	records.RemoveAll(q => q.dueDate < new DateTime(2019, 09, 01));

	int count = records.Count.Dump("Transactions Details");
	int i = 0;
	foreach (var group in records.GroupBy(q => new { q.vendorNumber, q.period, q.td_description }))
	{
		//group.Dump();

		var transaction = new Cohub.Data.Fin.Transaction(group.Min(g => g.startDate.Value))
		{
			//DueDate = group.Min(g => g.dueDate.Value),
			//InvoiceDate = group.Min(g => g.startDate.Value),
			//OrganizationId = group.Key.vendorNumber,
			//StatusId = group.Where(g => g.is_payment).Sum(g => g.amount) == group.Where(g => !g.is_payment).Sum(g => g.amount) ? InvoiceStatusId.Paid : InvoiceStatusId.Open,
			//TotalAmountDue = group.Where(g => !g.is_payment).Sum(g => g.amount.Value),
		};
		db.Add(transaction);

		foreach (var record in group)
		{
			transaction.TransactionDetails.Add(new TransactionDetail
			{
				Operation = db.GetOrAddTransactionType(record.transactionType),
				Category = db.GetOrAddGlGroup(record.td_description),
				Period = db.GetOrAddPeriod(record.period),
				Amount = record.amount ?? 0,
				Note = null,
			});
		}

		//		var paymentGroup = group.Where(g => g.is_payment);
		//		if (paymentGroup.Any())
		//		{
		//			foreach (var post in paymentGroup.GroupBy(o => o.effectiveDate.Value))
		//			{
		//				var td_ids = post.Select(p => p.transdetails_id);
		//
		//				var totalAmountPaid = post.Sum(p => p.amount) ?? 0;
		//
		//				// Can't figure out how to narrow down to just one payment
		//				//var innopayments = innoDb.PAYMENTs.Where(p => innoDb.PAYMENT_DETAILs.Any(pd => p.ID == pd.PAYMENT_ID && td_ids.Contains(pd.DETAIL_ID))).ToArray();
		//				//var innopayment = innopayments.SingleOrDefault();
		//
		//				var payment = new Payment
		//				{
		//					Created = post.Select(p => p.captureDate).Union(post.Select(p => p.effectiveDate)).Where(p => p != null).Select(p => p.Value).Min(),
		//					PaymentDate = post.Key,
		//					PaymentMethod = db.GetOrAddPaymentMethod("Unknown"),
		//					TotalAmountPaid = post.Sum(p => p.amount) ?? 0,
		//				};
		//				invoice.Payments.Add(payment);
		//				foreach (var pi in post)
		//				{
		//					payment.PaymentDetails.Add(new PaymentDetail
		//					{
		//						AmountPaid = pi.amount ?? 0,
		//						TransactionType = db.GetOrAddTransactionType(pi.transactionType),
		//						Account = db.GetOrAddGlGroup(pi.td_description),
		//						Period = db.GetOrAddPeriod(pi.period),
		//						Notes = null,
		//					});
		//				}
		//			}
		//		}

		//invoice.Dump();

		//invoice.InvoiceItems.Add(new InvoiceItem
		//{
		//	AccountId = record.tr
		//});

		if (i % 5000 == 0)
		{
			db.SaveChanges();
		}

		i++;
		Util.Progress = (int)(100 * (i / (float)count));
	}
	//db.SaveChanges();


}

public static class Extensions
{
	public static Category GetOrAddGlGroup(this CohubDbContext db, string td_description)
	{
		var account = db.Set<Category>().Find(td_description);

		if (account == null)
		{
			account = new Category
			{
				Id = td_description,
				IsActive = false,
				Description = null,
			};
			db.Add(account);
		}

		return account;
	}

	public static Operation GetOrAddTransactionType(this CohubDbContext db, string transactionType)
	{
		var invoiceTypeId = transactionType.Titleize();
		var entity = db.Set<Operation>().Find(invoiceTypeId);

		if (entity == null)
		{
			entity = new Operation
			{
				Id = invoiceTypeId,
				IsActive = false,
				Description = null,
			};
			db.Add(entity);
		}

		return entity;
	}

	public static Period GetOrAddPeriod(this CohubDbContext db, FILINGPERIOD source)
	{
		var periodId = $"{source.year}{source.filingSchedule.name[0]}{source.period:D2}";
		var entity = db.Set<Period>().Find(periodId);

		if (entity == null)
		{
			entity = new Period
			{
				Id = periodId,
				Name = source.description,
				DueDate = source.dueDate.Value,
				FromDate = source.startDate.Value,
				UntilDate = source.endDate.Value,
				Schedule = db.GetOrAddSchedule(source.filingSchedule),
			};
			db.Add(entity);
		}

		return entity;
	}

	public static Schedule GetOrAddSchedule(this CohubDbContext db, FILINGSCHEDULE source)
	{
		var entity = db.Set<Schedule>().Find(source.name);

		if (entity == null)
		{
			entity = new Schedule(source.timestamp.Value)
			{
				Id = source.name,
				IsActive = false,
				Description = source.alternateDescription,
			};
			db.Add(entity);
		}

		return entity;
	}

	public static PaymentMethod GetOrAddPaymentMethod(this CohubDbContext db, string paymentMethod)
	{
		if (string.IsNullOrWhiteSpace(paymentMethod))
		{
			return null;
		}

		var paymentMethodId = paymentMethod.Titleize();
		var entity = db.Set<PaymentMethod>().Find(paymentMethodId);

		if (entity == null)
		{
			entity = new PaymentMethod
			{
				Id = paymentMethodId,
				IsActive = false,
				Description = null,
			};
			db.Add(entity);
		}

		return entity;
	}

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