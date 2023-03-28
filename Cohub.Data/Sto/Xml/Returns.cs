using System.Collections.Generic;
using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "returns")]
	public class Returns
	{
		[XmlElement(ElementName = "Return")]
		public List<ReturnXml> Return { get; set; }
		[XmlAttribute(AttributeName = "downloaded")]
		public string Downloaded { get; set; }
	}
}
