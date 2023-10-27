using System.Collections.Generic;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="IDictionary{TKey,TValue}" />s.
/// </summary>
public static class DictionaryExtension
{
    /// <summary>
    ///     Adds or update an element with the provided key and value to the <see cref="IDictionary{TKey,TValue}" />
    /// </summary>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;

            return;
        }

        dictionary.Add(key, value);
    }
}
