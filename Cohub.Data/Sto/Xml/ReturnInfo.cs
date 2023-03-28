using System;
using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "return_info")]
    public class ReturnInfo
    {
        /// <summary>
        /// Maps to a <see cref="Fin.Category"/>.
        /// </summary>
        [XmlElement(ElementName = "Return_Code")]
        public string Return_Code { get; set; }
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }

        /// <summary>
        /// Corresponds to a <see cref="Rev.Filing.SchedulePeriod"/>.
        /// </summary>
        [XmlElement(ElementName = "file_period")]
        public string File_period { get; set; }
        public DateTimeOffset File_period_DateTimeOffset() => DateTimeOffset.Parse(File_period);

        [XmlElement(ElementName = "file_date")]
        public string File_date { get; set; }
        public DateTimeOffset File_date_DateTimeOffset() => DateTimeOffset.Parse(File_date);

        [XmlElement(ElementName = "amount_due")]
        public decimal Amount_due { get; set; }
        [XmlElement(ElementName = "taxpayerID_number")]
        public string TaxpayerID_number { get; set; }
        /// <summary>
        /// Corresponds to a <see cref="Rev.SchedulePeriod.Schedule"/>.
        /// </summary>
        [XmlElement(ElementName = "filing_status")]
        public string Filing_status { get; set; }
        [XmlElement(ElementName = "status_id")]
        public string Status_id { get; set; }
        [XmlElement(ElementName = "IsAmended")]
        public string IsAmended { get; set; }
        [XmlElement(ElementName = "old_ID")]
        public string Old_ID { get; set; }
        [XmlElement(ElementName = "old_file_date")]
        public string Old_file_date { get; set; }
        [XmlElement(ElementName = "old_amount_due")]
        public string Old_amount_due { get; set; }
        [XmlElement(ElementName = "final_return")]
        public string Final_return { get; set; }
    }
}
