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
</Query>

string connectionString = Util.GetPassword("anywhereusa_cohub_connection_string");
const bool reset = true;

void Main()
{
	var db = new CohubDbContext(new DbContextOptionsBuilder<CohubDbContext>().UseNpgsql(connectionString).Options, Actor.System);
	var innoDb = this;

	if (reset) // && Util.ReadLine<bool>("Reset?", true))
	{
		db.Database.ExecuteSqlInterpolated($"delete from fin.transaction_detail; delete from lic.license; delete from org.organization_comment; delete from org.organization");
	}

	// Mapping from account number to legacy account ID
	var accountNumberToAccountId = innoDb.ACCOUNTs.ToDictionary(a => a.vendor.vendorNumber, a => a.ID);
	var migratedAccountIds = db.Organizations().Select(o => accountNumberToAccountId[o.Id]).ToList();

	var query =
		from a in innoDb.ACCOUNTs.Where(x => !migratedAccountIds.Contains(x.ID))
		let v = a.vendor
		join b in innoDb.BUSINESSes on v.business_ID equals (long)b.ID
		let dba = v.primaryDBA
		let email = innoDb.EMAILADDRESSes.FirstOrDefault(x => x.ID == v.email_ID)
		let phone = innoDb.PHONENUMBERs.FirstOrDefault(x => x.ID == v.phoneNumber_ID)
		let naics = innoDb.NAICS.FirstOrDefault(x => x.ID == v.naics_ID)
		let physical_id = v.address_ID
		let mailing_id = innoDb.CONTACT_VENDORs
			.Where(cv => cv.vendors_ID == v.ID)
			.SelectMany(cv => cv.contacts.CONTACT_ADDRESSes.OrderByDescending(ca => ca.Contact.dateCreated).Select(ca => (long)ca.addresses_ID))
			.FirstOrDefault()
		//let address_ids = innoDb.CONTACT_VENDORs
		//	.Where(cv => cv.vendors_ID == v.ID)
		//	.SelectMany(cv => cv.contacts.CONTACT_ADDRESSes.Select(ca => new { address_id = (long)ca.addresses_ID }))
		//	.ToArray()
		select new
		{
			//address_ids,
			ACCOUNT_ID = a.ID,
			VENDOR_STATEID = v.stateId,
			BUSINESS_ID = b.ID,
			BUSINESS_TIMESTAMP = b.timestamp.Value,
			BUSINESS_FEIN = b.FEIN,
			CONTACT_IDs = b.CONTACTs.Select(contact => (long)contact.ID),
			PHYSICAL_ID = physical_id ?? 0,
			MAILING_ID = mailing_id,
			GEOCODE_CODE = v.geoCode.code,
			GEOCODE_DESCRIPTION = v.geoCode.description,
			NAICS_CODE = naics.naicsCode,
			LICENSE_IDs = a.LICENSEs.Select(license => license.ID),
			Organization = new Organization(a.timestamp.Value)
			{
				Id = v.vendorNumber,
				StatusId =
					a.status == "A" ? OrganizationStatusId.Active :
					a.status == "P" ? OrganizationStatusId.Pending :
					OrganizationStatusId.Occasional,
				//Labels = { },
				OrganizationName = b.name,
				Dba = dba.name == b.name ? null : dba.name,
				OrganizationDescription = a.description,
				OrganizationEmail = email.value,
				OrganizationPhoneNumber = phone.value,
				ClassificationId =
					v.peddler == true ? OrganizationClassificationId.MobileVendor :
					v.homeBased == true ? OrganizationClassificationId.HomeBased :
					v.inCity == true ? OrganizationClassificationId.Commercial :
					OrganizationClassificationId.OutOfCity,
				TypeId =
					b.ownershipType == "C" ? OrganizationTypeId.Corporation :
					b.ownershipType == "I" ? OrganizationTypeId.International :
					b.ownershipType == "LLC" ? OrganizationTypeId.LLC :
					b.ownershipType == "LLP" ? OrganizationTypeId.LLP :
					b.ownershipType == "O" ? OrganizationTypeId.Other :
					b.ownershipType == "P" ? OrganizationTypeId.Partnership :
					OrganizationTypeId.Unknown,
			}
		};

	var addresses = innoDb.ADDRESSes.ToDictionary(addr => addr.ID);
	$"Materialized {addresses.Count} addresses".Dump();

	var contacts = innoDb.CONTACTs.ToDictionary(o => (long)o.ID);
	$"Materialized {contacts.Count} contacts".Dump();

	var licenses = innoDb.LICENSEs.ToDictionary(o => o.ID);
	$"Materialized {licenses.Count} licenses".Dump();

	var geocodes = innoDb.GEOCODEs.ToDictionary(o => o.ID);
	$"Materialized {geocodes.Count} geocodes".Dump();

	int count = query.Count();
	int i = 0;

	var list = query.ToList().Dump();
	db.AddRange(list.Select(l => l.Organization));

	$"Migrating {count} Organizations".Dump();
	foreach (var record in list)
	{
		var org = record.Organization;

		if (record.GEOCODE_CODE != null)
		{
			db.Add(new OrganizationLabel()
			{
				OrganizationId = org.Id,
				LabelId = db.GetOrAddLabel("Geocode: " + record.GEOCODE_CODE, record.GEOCODE_DESCRIPTION).Id,
			});
		}

		if (record.NAICS_CODE != null)
		{
			db.Add(new OrganizationLabel()
			{
				OrganizationId = org.Id,
				LabelId = db.GetOrAddLabel("NAICS: " + record.NAICS_CODE, null).Id,
			});
		}

		if (addresses.TryGetValue(record.PHYSICAL_ID, out var physical) && physical != null)
		{
			org.PhysicalAddress = new Address
			{
				AddressLines = physical.addressLine1 + (string.IsNullOrWhiteSpace(physical.addressLine2) ? "" : ("\n" + physical.addressLine2)),
				City = physical.city,
				StateId = physical.state,
				Zip = physical.zipCode?.Substring(0, Math.Min(10, physical.zipCode.Length)),
			};
		}

		if (addresses.TryGetValue(record.MAILING_ID, out var mailing) && mailing != null)
		{
			org.MailingAddress = new Address
			{
				AddressLines = mailing.addressLine1 + (string.IsNullOrWhiteSpace(mailing.addressLine2) ? "" : ("\n" + mailing.addressLine2)),
				City = mailing.city,
				StateId = mailing.state,
				Zip = mailing.zipCode.Substring(0, Math.Min(10, mailing.zipCode.Length)),
			};
		}

		if (org.MailingAddress.IsEmpty)
		{
			org.MailingAddress.UpdateWith(org.PhysicalAddress);
		}
		else if (org.PhysicalAddress.IsEmpty)
		{
			org.PhysicalAddress.UpdateWith(org.MailingAddress);
		}

		if (!string.IsNullOrWhiteSpace(record.VENDOR_STATEID))
		{
			org.StateID = record.VENDOR_STATEID;
		}

		if (!string.IsNullOrWhiteSpace(record.BUSINESS_FEIN))
		{
			org.FederalID = record.BUSINESS_FEIN;
		}

		foreach (var contactId in record.CONTACT_IDs)
		{
			contacts.TryGetValue(contactId, out var contact);
			var orgContact = new OrganizationContact
			{
				OrganizationId = org.Id,
				LegalName = contact.name.name,
				Email = contact.primaryEmailAddress?.value,
				PhoneNumber = contact.primaryPhoneNumber?.value,
				RelationshipId =
					contact.TYPE == "Owner" ? RelationshipId.Officer :
					contact.TYPE == "Contact" ? RelationshipId.Contact :
					RelationshipId.Unknown,
			};
			if (contact.primaryAddress_ID.HasValue && addresses.TryGetValue((long)contact.primaryAddress_ID, out ADDRESS primaryAddress))
			{
				orgContact.Address.UpdateWith(!org.PhysicalAddress.IsEmpty ? org.PhysicalAddress : org.MailingAddress);
			}
			db.Add(orgContact);
		}

		foreach (var licenseId in record.LICENSE_IDs)
		{
			licenses.TryGetValue(licenseId, out var l);
			db.Add(new Cohub.Data.Lic.License(l.initialReceivedDate.Value)
			{
				Title = l.licenseNo,
				OrganizationId = org.Id,
				TypeId = LicenseTypeId.Business,
				IssuedDate = l.issuedDate.Value,
				ExpirationDate = l.terminateDate ?? l.expirationDate.Value,
				Description = "Legacy Status: " + l.status,
			});
		}

		//try
		//{
		//	db.SaveChanges();
		//}
		//catch (Exception ex)
		//{
		//	ex.Dump("Saving Changes");
		//}

		i++;
		Util.Progress = (int)(100 * (i / (float)count));
	}
	db.SaveChanges();
	"Finished Migrating Organizations".Dump();
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
}