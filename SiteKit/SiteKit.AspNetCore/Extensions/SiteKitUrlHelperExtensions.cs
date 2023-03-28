using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitUrlHelperExtensions
    {
        /// <summary>
        /// Returns the value of "Buster" from <see cref="IConfiguration"/>.
        /// Useful for cache busting URLs.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Buster(this IUrlHelper url)
        {
            return url.ActionContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Buster"] ?? string.Empty;
        }

        /// <summary>
        /// Returns a current request's base URL (scheme and host) with 
        /// the optional <paramref name="rightPart"/> appended to the end.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="rightPart"></param>
        /// <returns></returns>
        public static string FullUrl(this IUrlHelper url, string rightPart = "")
        {
            var request = url.ActionContext.HttpContext.Request;
            var fullBase = $"{request.Scheme}://{request.Host}/{rightPart.TrimStart('/')}";
            return fullBase;
        }

        /// <summary>
        /// Returns the current URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string CurrentUrl(this IUrlHelper url)
        {
            var request = url.ActionContext.HttpContext.Request;
            var current = $"{request.PathBase}{request.Path}{request.QueryString}";
            return current;
        }

        public static string ActionMergedWithCurrentUrl(this IUrlHelper url, string action, string? controller = null, object? values = null)
        {
            if (action is null)
                throw new System.ArgumentNullException(nameof(action));

            var routeValues = new RouteValueDictionary(values);
            foreach (var item in url.ActionContext.RouteData.Values)
            {
                if (!routeValues.ContainsKey(item.Key))
                    routeValues.Add(item.Key, item.Value);
            }
            foreach (var item in url.ActionContext.HttpContext.Request.Query)
            {
                if (!routeValues.ContainsKey(item.Key))
                    routeValues.Add(item.Key, item.Value);
            }
            return url.Action(action, controller, values: routeValues);
        }

        /// <summary>
        /// Returns a URL with a "returnUrl" that is the current URL.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ActionWithReturnUrl(this IUrlHelper url, string action, object values)
        {
            if (action is null)
                throw new System.ArgumentNullException(nameof(action));
            if (values is null)
                throw new System.ArgumentNullException(nameof(values));

            return url.ActionWithReturnUrl(action: action, controller: null, values: values);
        }

        /// <summary>
        /// Returns a URL with a "returnUrl" that is the current URL.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="values"></param>
        /// <param name="returnUrlFragment"></param>
        /// <returns></returns>
        public static string ActionWithReturnUrl(this IUrlHelper url, string action, string? controller = null, object? values = null, string? returnUrlFragment = null)
        {
            if (action is null)
                throw new System.ArgumentNullException(nameof(action));

            var routeValues = new RouteValueDictionary(values)
            {
                // Pass through the returnUrl if already set,
                // otherwise generate the returnUrl
                ["returnUrl"] = url.ActionContext.HttpContext.Request.Query.TryGetValue("returnUrl", out var returnUrl) ?
                    returnUrl :
                    (url.CurrentUrl() + (returnUrlFragment != null ? $"#{returnUrlFragment}" : string.Empty))
            };
            return url.Action(action, controller, values: routeValues);
        }

        /// <summary>
        /// Returns the value of "returnUrl" from the query string if it exists and is local.
        /// Otherwise returns <c>null</c>.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string? ReturnUrl(this IUrlHelper url)
        {
            return url.ActionContext.HttpContext.Request.Query.TryGetValue("returnUrl", out var returnUrl) && url.IsLocalUrl(returnUrl) ?
                returnUrl.ToString() :
                null;
        }

        /// <summary>
        /// Returns the "returnUrl" in the query string if it is present and local, or the routed URL.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReturnUrlOrAction(this IUrlHelper url, string action, object values)
        {
            if (action is null)
                throw new System.ArgumentNullException(nameof(action));
            if (values is null)
                throw new System.ArgumentNullException(nameof(values));

            return url.ReturnUrl() ?? url.Action(action, values);
        }

        /// <summary>
        /// Returns the "returnUrl" in the query string if it is present and local, or the routed URL.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ReturnUrlOrAction(this IUrlHelper url, string action, string? controller = null, object? values = null)
        {
            if (action is null)
                throw new System.ArgumentNullException(nameof(action));

            return url.ReturnUrl() ?? url.Action(action, controller, values);
        }
    }
}
