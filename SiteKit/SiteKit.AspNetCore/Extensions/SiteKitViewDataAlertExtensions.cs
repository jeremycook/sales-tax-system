using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SiteKit.Info;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitViewDataAlertExtensions
    {
        private static readonly string AlertsViewDataKey = typeof(Alert).FullName!;

        /// <summary>
        /// Returns all <see cref="Alert"/>s from <paramref name="viewData"/>.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static List<Alert> PeekAlerts(this ViewDataDictionary viewData)
        {
            if (!viewData.TryGetValue(AlertsViewDataKey, out var alerts))
            {
                alerts = new List<Alert>();
                viewData[AlertsViewDataKey] = alerts;
            }

            return (List<Alert>)alerts;
        }

        /// <summary>
        /// Returns all <see cref="Alert"/>s from <paramref name="viewData"/>
        /// and clears the <see cref="Alert"/>s in <paramref name="viewData"/>.
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static List<Alert> Alerts(this ViewDataDictionary viewData)
        {
            var alerts = viewData.PeekAlerts();
            var copy = alerts.ToList();
            alerts.Clear();
            return copy;
        }

        public static void AddAlert(this ViewDataDictionary viewData, Alert alert)
        {
            viewData.PeekAlerts().Add(alert);
        }

        /// <summary>
        /// Add an <see cref="Alert"/> to <paramref name="viewData"/> with the "warning" category.
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="message"></param>
        public static void Warn(this ViewDataDictionary viewData, string? message)
        {
            viewData.AddAlert(new Alert
            {
                Message = SiteKit.Text.Html.Encode(message),
                Category = "warning"
            });
        }

        public static void Warn(this ViewDataDictionary viewData, SiteKit.Text.Html html)
        {
            viewData.AddAlert(new Alert
            {
                Message = html,
                Category = "warning"
            });
        }

        /// <summary>
        /// Add an <see cref="Alert"/> to <paramref name="viewData"/> with the "success" category.
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="message"></param>
        public static void Success(this ViewDataDictionary viewData, string? message)
        {
            viewData.AddAlert(new Alert
            {
                Message = SiteKit.Text.Html.Encode(message),
                Category = "success"
            });
        }

        public static void Success(this ViewDataDictionary viewData, SiteKit.Text.Html html)
        {
            viewData.AddAlert(new Alert
            {
                Message = html,
                Category = "success"
            });
        }
    }
}
