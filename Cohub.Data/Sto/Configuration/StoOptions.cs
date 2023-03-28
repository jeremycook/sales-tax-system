using System.Collections.Generic;

namespace Cohub.Data.Sto.Configuration
{
    public class StoOptions
    {
        public Dictionary<string, string> ReturnCodeToCategoryMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FilingStatusToFrequencyMap { get; set; } = new Dictionary<string, string>();
    }
}
