using System;

namespace Microsoft.AspNetCore.Routing
{
    public static class SiteKitRouteValueDictionaryExtensions
    {
        private const string actionKey = "action";
        private const string create = "create";

        /// <summary>
        /// Returns <c>true</c> if <paramref name="routeValues"/> contains create:action key:value pair.
        /// </summary>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static bool CreateAction(this RouteValueDictionary routeValues)
        {
            return
                routeValues.TryGetValue(actionKey, out var value) &&
                value is string action &&
                action.Equals(create, StringComparison.OrdinalIgnoreCase);
        }
    }
}
