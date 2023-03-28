using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SiteKit.Info;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitTempDataExtensions
    {
        private static readonly string AlertsTempDataKey = typeof(Alert).FullName!;

        public static void SetAlerts(this ITempDataDictionary tempData, IEnumerable<Alert> alerts)
        {
            string json = JsonSerializer.Serialize(alerts);
            // Limit payload to 4K since this gets stored in a cookie
            if (Encoding.UTF8.GetByteCount(json) <= 1024)
            {
                tempData[AlertsTempDataKey] = json;
            }
        }

        /// <summary>
        /// Returns all <see cref="Alert"/>s from <paramref name="tempData"/> using 
        /// <see cref="ITempDataDictionary.Peek(string)"/> method.
        /// </summary>
        /// <param name="tempData"></param>
        /// <returns></returns>
        public static IEnumerable<Alert> PeekAlerts(this ITempDataDictionary tempData)
        {
            if (tempData.Peek(AlertsTempDataKey) is string json &&
                JsonSerializer.Deserialize<List<Alert>>(json) is IEnumerable<Alert> alerts)
            {
                return alerts;
            }
            else
            {
                return Enumerable.Empty<Alert>();
            }
        }

        /// <summary>
        /// Returns all <see cref="Alert"/>s from <paramref name="tempData"/>
        /// and clears the <see cref="Alert"/>s in <paramref name="tempData"/>.
        /// </summary>
        /// <param name="tempData"></param>
        /// <returns></returns>
        public static IEnumerable<Alert> Alerts(this ITempDataDictionary tempData)
        {
            if (tempData[AlertsTempDataKey] is string json &&
                JsonSerializer.Deserialize<List<Alert>>(json) is IEnumerable<Alert> alerts)
            {
                tempData.SetAlerts(Enumerable.Empty<Alert>());
                return alerts;
            }
            else
            {
                return Enumerable.Empty<Alert>();
            }
        }

        public static void AddAlert(this ITempDataDictionary tempData, Alert alert)
        {
            tempData.SetAlerts(tempData.PeekAlerts().Append(alert));
        }

        /// <summary>
        /// Add an <see cref="Alert"/> to <paramref name="tempData"/> with the "warning" category.
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="message"></param>
        public static void Warn(this ITempDataDictionary tempData, string? message)
        {
            tempData.AddAlert(new Alert
            {
                Message = SiteKit.Text.Html.Encode(message),
                Category = "warning"
            });
        }

        public static void Warn(this ITempDataDictionary tempData, SiteKit.Text.Html html)
        {
            tempData.AddAlert(new Alert
            {
                Message = html,
                Category = "warning"
            });
        }

        /// <summary>
        /// Adds <see cref="Alert"/>s to <paramref name="tempData"/> with the "error" category.
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="modelState"></param>
        public static void Error(this ITempDataDictionary tempData, ModelStateDictionary modelState)
        {
            foreach (var error in modelState.SelectMany(o => o.Value.Errors))
            {
                tempData.AddAlert(new Alert
                {
                    Message = SiteKit.Text.Html.Encode(error.ErrorMessage),
                    Category = "error"
                });
            }
        }

        /// <summary>
        /// Add an <see cref="Alert"/> to <paramref name="tempData"/> with the "success" category.
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="message"></param>
        public static void Success(this ITempDataDictionary tempData, string? message)
        {
            tempData.AddAlert(new Alert
            {
                Message = SiteKit.Text.Html.Encode(message),
                Category = "success"
            });
        }

        public static void Success(this ITempDataDictionary tempData, SiteKit.Text.Html html)
        {
            tempData.AddAlert(new Alert
            {
                Message = html,
                Category = "success"
            });
        }
    }
}
