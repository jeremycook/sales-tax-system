using System;
using Cohub.Data.Geo;
using Cohub.Data.Org;
using Cohub.Data.Sto.Xml;
using SiteKit.Info;
using System.Collections.Generic;
using System.Linq;

namespace Cohub.Data.Sto.Services
{
    public class StoReturnToOrganizationMapper
    {
        public void Map(Organization org, ReturnXml sto)
        {
            org.ClassificationId = OrganizationClassificationId.Unclassified;
            org.TypeId = OrganizationTypeId.Unknown;

            org.OrganizationName = sto.Taxpayer_info.Company;
            org.Dba = null;
            org.OnlineFiler = true;
            org.SendPhysicalMail = false;
            org.OrganizationDescription = null;
            org.Restrictions = null;
            org.OrganizationEmail = sto.Taxpayer_info.Email;
            org.OrganizationPhoneNumber = sto.Taxpayer_info.Phone;

            org.PhysicalAddress = new Address
            {
                AddressLines =
                    sto.Taxpayer_info.Mail_address1 +
                    (!string.IsNullOrEmpty(sto.Taxpayer_info.Mail_address2) ? "\n" + sto.Taxpayer_info.Mail_address2 : null),
                City = sto.Taxpayer_info.Mail_city,
                StateId = sto.Taxpayer_info.Mail_state,
                Zip = sto.Taxpayer_info.Mail_zip,
            };
            org.MailingAddress = new Address
            {
                AddressLines =
                    sto.Taxpayer_info.Business_address1 +
                    (!string.IsNullOrEmpty(sto.Taxpayer_info.Business_address2) ? "\n" + sto.Taxpayer_info.Business_address2 : null),
                City = sto.Taxpayer_info.Business_city,
                StateId = sto.Taxpayer_info.Business_state,
                Zip = sto.Taxpayer_info.Business_zip,
            };

            org.StateID = sto.Taxpayer_info.Statetaxid.Nullify();
            org.FederalID = sto.Taxpayer_info.FedEmplID.Nullify();

            org.Contacts = new List<OrganizationContact>
            {
                new OrganizationContact
                {
                    LegalName = $"{sto.Taxpayer_info.First_name} {sto.Taxpayer_info.Last_name}",
                    RelationshipId = RelationshipId.Officer,
                    Address = new Address(org.PhysicalAddress),
                }
            };
        }
    }
}