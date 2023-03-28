using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace Microsoft.AspNetCore.Mvc
{
    public static class SiteKitHtmlHelperExtensions
    {
        public static IHtmlContent Br(this IHtmlHelper htmlHelper, string plainText)
        {
            return htmlHelper.Raw(SiteKit.Text.Html.Br(plainText).RawHtml);
        }

        public static ModelMetadata MetadataFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
        {
            var modelExpressionProvider = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelExpressionProvider>();

            return modelExpressionProvider.CreateModelExpression(htmlHelper.ViewData, expression).Metadata;
        }

        public static IHtmlContent PartialFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string partialName)
        {
            var modelExpressionProvider = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelExpressionProvider>();

            var modelExpression = modelExpressionProvider.CreateModelExpression(html.ViewData, expression);

            ViewDataDictionary<TProperty> viewData = new ViewDataDictionary<TProperty>(html.ViewData, modelExpression.Model)
            {
                ModelExplorer = modelExpression.ModelExplorer,
                TemplateInfo = {
                    HtmlFieldPrefix = modelExpression.Name
                }
            };

            return html.Partial(partialName, modelExpression.Model, viewData);
        }

        public static IHtmlContent PartialFor<TModel, TProperty>(this IHtmlHelper<TModel> html, string propertyName, TProperty propertyModel, string partialName)
        {
            var propertyExplorer = html.ViewData.ModelExplorer.GetExplorerForProperty(propertyName);

            ViewDataDictionary<TProperty> viewData = new ViewDataDictionary<TProperty>(html.ViewData, propertyModel)
            {
                ModelExplorer = propertyExplorer,
                TemplateInfo = {
                    HtmlFieldPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix + (html.ViewData.TemplateInfo.HtmlFieldPrefix.IsNullOrWhiteSpace() ? "" : ".") + propertyName
                }
            };

            return html.Partial(partialName, propertyModel, viewData);
        }

        /// <summary>
        /// Returns the name of the partial. Request for a partial to be rendered once by the top-most layout.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="partialName"></param>
        /// <returns></returns>
        public static string RequestPartial(this IHtmlHelper htmlHelper, string partialName)
        {
            return htmlHelper.ViewContext.HttpContext.RequestPartial(partialName);
        }

        /// <summary>
        /// Serializes <see cref="data"/> to JSON without using the ASP.NET Core options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Serialize<T>(this IHtmlHelper htmlHelper, T data, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(data, options);
        }
    }
}
