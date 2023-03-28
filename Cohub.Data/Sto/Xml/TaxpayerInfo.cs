using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "taxpayer_info")]
	public class TaxpayerInfo
	{
		[XmlElement(ElementName = "first_name")]
		public string First_name { get; set; }
		[XmlElement(ElementName = "last_name")]
		public string Last_name { get; set; }
		[XmlElement(ElementName = "mail_address1")]
		public string Mail_address1 { get; set; }
		[XmlElement(ElementName = "mail_address2")]
		public string Mail_address2 { get; set; }
		[XmlElement(ElementName = "mail_city")]
		public string Mail_city { get; set; }
		[XmlElement(ElementName = "mail_state")]
		public string Mail_state { get; set; }
		[XmlElement(ElementName = "mail_zip")]
		public string Mail_zip { get; set; }
		[XmlElement(ElementName = "business_address1")]
		public string Business_address1 { get; set; }
		[XmlElement(ElementName = "business_address2")]
		public string Business_address2 { get; set; }
		[XmlElement(ElementName = "business_city")]
		public string Business_city { get; set; }
		[XmlElement(ElementName = "business_state")]
		public string Business_state { get; set; }
		[XmlElement(ElementName = "business_zip")]
		public string Business_zip { get; set; }
		[XmlElement(ElementName = "statetaxid")]
		public string Statetaxid { get; set; }
		[XmlElement(ElementName = "phone")]
		public string Phone { get; set; }
		[XmlElement(ElementName = "fax")]
		public string Fax { get; set; }
		[XmlElement(ElementName = "email")]
		public string Email { get; set; }
		[XmlElement(ElementName = "company")]
		public string Company { get; set; }
		[XmlElement(ElementName = "naicscode")]
		public string Naicscode { get; set; }
		[XmlElement(ElementName = "FedEmplID")]
		public string FedEmplID { get; set; }
	}
}
