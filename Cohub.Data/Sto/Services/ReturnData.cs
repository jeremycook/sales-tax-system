using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Sto.Xml;

namespace Cohub.Data.Sto.Services
{
    public class ReturnData
    {
        public ReturnXml StoReturn { get; set; } = null!;
        public Organization Organization { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public Period Period { get; set; } = null!;
        public Return Return { get; set; } = null!;

        public override string ToString()
        {
            return StoReturn?.ToString() ?? base.ToString();
        }
    }
}
