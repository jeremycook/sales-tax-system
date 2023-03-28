using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SiteKit.Text
{
    public class Html
    {
        public static HtmlParser Parser { get; } = new HtmlParser();

        public static Ganss.Xss.HtmlSanitizer Sanitizer { get; } = new Ganss.Xss.HtmlSanitizer();
        public static Html Empty { get; } = new Html(string.Empty);

        public string RawHtml { get; set; }

        public Html(string rawHtml)
        {
            RawHtml = rawHtml;
        }

        /// <summary>
        /// Formats <paramref name="format"/> with <paramref name="args"/> HTML encoded.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Html Format(string format, IEnumerable<object> args)
        {
            return new Html(string.Format(format, args: args.Select(arg => arg is Html ? arg.ToString() : HttpUtility.HtmlEncode(arg)).ToArray()));
        }

        /// <summary>
        /// HTML encodes the arguments of <paramref name="formattableString"/>.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Html Interpolate(FormattableString formattableString)
        {
            return Format(formattableString.Format, args: formattableString.GetArguments());
        }

        public static Html Br(string? html)
        {
            return new Html(string.Join("<br/>", System.Text.RegularExpressions.Regex.Split(html ?? string.Empty, "\r?\n").Select(Encode)));
        }

        public static Html Encode(string? html)
        {
            return new Html(HttpUtility.HtmlEncode(html ?? string.Empty));
        }

        public static Html Sanitize(string? html)
        {
            return new Html(Sanitizer.Sanitize(html ?? string.Empty));
        }

        public static string Strip(string? html)
        {
            return Parser.ParseDocument(html ?? string.Empty).DocumentElement.Text();
        }

        /// <summary>
        /// Returns the HTML.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return RawHtml ?? string.Empty;
        }
    }
}
