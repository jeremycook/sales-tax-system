namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class SiteKitJsonHelperExtensions
    {
        public static string SerializeToString(this IJsonHelper json, object value)
        {
            return json.Serialize(value).ToString() ?? string.Empty;
        }
    }
}
