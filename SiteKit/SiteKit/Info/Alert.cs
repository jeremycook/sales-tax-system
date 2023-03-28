using SiteKit.Text;
using System;

namespace SiteKit.Info
{
    public class Alert
    {
        public Html Message { get; set; } = Html.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
