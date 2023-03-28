using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "payment_info")]
    public class PaymentInfo
    {
        [XmlElement(ElementName = "account_type")]
        public string Account_type { get; set; }
        [XmlElement(ElementName = "first_name")]
        public string First_name { get; set; }
        [XmlElement(ElementName = "last_name")]
        public string Last_name { get; set; }
        [XmlElement(ElementName = "is_business")]
        public string Is_business { get; set; }
        [XmlElement(ElementName = "address1")]
        public string Address1 { get; set; }
        [XmlElement(ElementName = "address2")]
        public string Address2 { get; set; }
        [XmlElement(ElementName = "city")]
        public string City { get; set; }
        [XmlElement(ElementName = "state")]
        public string State { get; set; }
        [XmlElement(ElementName = "zip")]
        public string Zip { get; set; }

        // No need to deserialize or store this data.
        //[XmlElement(ElementName = "bank_account_number")]
        //public string Bank_account_number { get; set; }
        //[XmlElement(ElementName = "routing_number")]
        //public string Routing_number { get; set; }
    }
}
