using System.Linq;

namespace System
{
    public static class SiteKitExceptionExtensions
    {
        /// <summary>
        /// List all messages from <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string AllMessages(this Exception ex)
        {
            return string.Join(" \n• ", ex.Flatten(o => o.InnerException).Select(o => o.Message));
        }

        /// <summary>
        /// Returns the message from the inner-most exception of <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string OriginalMessage(this Exception ex)
        {
            return ex.Flatten(o => o.InnerException).Select(o => o.Message).LastOrDefault();
        }
    }
}
