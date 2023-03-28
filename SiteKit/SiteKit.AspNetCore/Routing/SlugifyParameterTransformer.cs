using Microsoft.AspNetCore.Routing;
using System;
using System.Text.RegularExpressions;

namespace SiteKit.AspNetCore.Routing
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        private static readonly Regex pattern = new Regex("([a-z])([A-Z])", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        public string? TransformOutbound(object? value)
        {
            if (value?.ToString() is string text)
            {
                return pattern.Replace(text, "$1-$2").ToLowerInvariant();
            }
            else
            {
                return null;
            }
        }
    }
}
