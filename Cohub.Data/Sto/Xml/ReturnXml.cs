using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "Return")]
    public class ReturnXml
    {
        [XmlElement(ElementName = "taxpayer_info")]
        public TaxpayerInfo Taxpayer_info { get; set; }
        [XmlElement(ElementName = "payment_info")]
        public PaymentInfo Payment_info { get; set; }
        [XmlElement(ElementName = "return_info")]
        public ReturnInfo Return_info { get; set; }
        [XmlElement(ElementName = "user_entries")]
        public UserEntries User_entries { get; set; }

        public override string ToString()
        {
            return $"{Return_info.ID}: {Return_info.TaxpayerID_number} {Taxpayer_info.Company}";
        }
    }
}
