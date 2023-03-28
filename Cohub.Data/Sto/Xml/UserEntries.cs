using System.Collections.Generic;
using System.Xml.Serialization;

#nullable disable

namespace Cohub.Data.Sto.Xml
{
    [XmlRoot(ElementName = "user_entries")]
	public class UserEntries
	{
		[XmlElement(ElementName = "user_entry")]
		public List<UserEntry> User_entry { get; set; }
	}
}
