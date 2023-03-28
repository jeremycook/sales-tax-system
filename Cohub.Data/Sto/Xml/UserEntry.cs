using System;
using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "user_entry")]
    public class UserEntry
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "val")]
        public string Val { get; set; }

        public decimal? AsDecimal => decimal.TryParse(Val, out var result) ? result : default(decimal?);
        public double? AsDouble => double.TryParse(Val, out var result) ? result : default(double?);
        public DateTimeOffset? AsDateTime => DateTimeOffset.TryParseExact(Val, "M/d/yyyy", null, System.Globalization.DateTimeStyles.AssumeLocal, out var result) ? result : default(DateTimeOffset?);
    }
}
