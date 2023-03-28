using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cohub.Data.Geo.Configuration
{
    public class GeoOptions
    {
        /// <summary>
        /// Defaults to "CO" unless modified.
        /// </summary>
        public string DefaultStateId { get; set; } = "CO";
    }
}
