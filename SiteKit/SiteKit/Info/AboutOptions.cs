using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteKit.Info
{
    public class AboutOptions
    {
        public string Title { get; set; } = "My Awesome Site";

        public string BaseUrl { get; set; } = null!;

        /// <summary>
        /// The default TZDB time zone.
        /// </summary>
        public string Zoneinfo { get; set; } = "America/Denver";
    }
}
