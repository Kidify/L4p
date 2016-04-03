using System.Collections.Generic;

namespace L4p.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            TValue value;

            if (!dict.TryGetValue(key, out value))
            {
                value = new TValue();
                dict.Add(key, value);
            }

            return value;
        }
    }
}
