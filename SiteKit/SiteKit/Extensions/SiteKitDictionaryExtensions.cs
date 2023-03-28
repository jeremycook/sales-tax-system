namespace System.Collections.Generic
{
    public static class SiteKitDictionaryExtensions
    {
        public static TValue? GetValueOrDefault<TValue>(this IDictionary<string, TValue> dictionary, string key)
            where TValue : struct
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            return dictionary.TryGetValue(key, out TValue value) ? value : null;
        }

        public static void SetOrRemoveValue<TValue>(this IDictionary<string, TValue> dictionary, string key, TValue? value)
            where TValue : struct
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (value is null)
            {
                dictionary.Remove(key);
            }
            else
            {
                dictionary[key] = value.Value;
            }
        }
    }
}
