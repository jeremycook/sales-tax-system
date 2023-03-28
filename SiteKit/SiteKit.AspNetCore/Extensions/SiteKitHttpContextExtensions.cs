using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitHttpContextExtensions
    {
        private static readonly object RequestPartialKey = new object();

        /// <summary>
        /// Request for a partial to be rendered once by the top-most layout.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="partialName"></param>
        public static string RequestPartial(this HttpContext httpContext, string partialName)
        {
            if (httpContext.Items[RequestPartialKey] is not HashSet<string> requestedPartials)
            {
                requestedPartials = new HashSet<string>();
                httpContext.Items[RequestPartialKey] = requestedPartials;
            }

            requestedPartials.Add(partialName);

            return Regex.Replace(partialName, @"[/\\]", "-");
        }

        /// <summary>
        /// Request for partials to be rendered once by the top-most layout.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="partialNames"></param>
        public static void RequestPartials(this HttpContext httpContext, params string[] partialNames)
        {
            if (httpContext.Items[RequestPartialKey] is not HashSet<string> requestedPartials)
            {
                requestedPartials = new HashSet<string>();
                httpContext.Items[RequestPartialKey] = requestedPartials;
            }

            requestedPartials.AddRange(partialNames);
        }

        /// <summary>
        /// List the partials that have been requested.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static IEnumerable<string> RequestedPartials(this HttpContext httpContext)
        {
            if (httpContext.Items[RequestPartialKey] is HashSet<string> requestedPartials)
            {
                return requestedPartials;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
