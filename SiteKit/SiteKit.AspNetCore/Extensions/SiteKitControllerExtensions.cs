namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitControllerExtensions
    {
        /// <summary>
        /// Redirects to the "returnUrl" in the query string if it is present and local, or the routed URL.
        /// </summary>
        /// <param name="controllerBase"></param>
        /// <param name="action"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static RedirectResult RedirectToReturnUrlOrAction(this ControllerBase controllerBase, string action, object values)
        {
            return controllerBase.Redirect(controllerBase.Url.ReturnUrlOrAction(action, values));
        }

        /// <summary>
        /// Redirects to the "returnUrl" in the query string if it is present and local, or the routed URL.
        /// </summary>
        /// <param name="controllerBase"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static RedirectResult RedirectToReturnUrlOrAction(this ControllerBase controllerBase, string action, string? controller = null, object? values = null)
        {
            return controllerBase.Redirect(controllerBase.Url.ReturnUrlOrAction(action, controller, values));
        }
    }
}
