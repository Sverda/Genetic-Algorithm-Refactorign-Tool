using System;
using System.Collections.Generic;

namespace GenSharp.Refactorings.Analyzers.Helpers.ExtractMethod
{
    internal static class DictionaryExtensions
    {
        public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> function)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = function(key);
            dictionary.Add(key, value);
            return value;
        }
    }
}
