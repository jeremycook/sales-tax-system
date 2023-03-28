namespace System
{
    public static class SiteKitStringExtensions
    {
        /// <summary>
        /// Return <c>null</c> if <paramref name="text"/> is null or just whitespace,
        /// otherwise returns the value as-is.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string? Nullify(this string? text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        /// <summary>
        /// Return <c>true</c> if <paramref name="text"/> is null or whitespace.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string? text)
        {
            return string.IsNullOrWhiteSpace(text);
        }
    }
}
