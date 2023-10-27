using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Maverick.Client.ArangoDb.Protocol.Extensions
{
    /// <summary>
    ///     Contains some extension methods of <see cref="HttpRequestOptions"/>
    /// </summary>
    internal static class HttpRequestOptionsExtensions
    {
        /// <summary>
        ///     This method is used to check whether the <see cref="HttpRequestOptions"/> contains the specified key or not.
        /// </summary>
        /// <param name="requestOptions"><see cref="HttpRequestOptions"/></param>
        /// <param name="key">  The key that is being checked</param>
        /// <returns>True if the key is contained in the <see cref="HttpRequestOptions"/> otherwise false.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool ContainsKey(this HttpRequestOptions requestOptions, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("The key should not be null or whitespace", nameof(key));
            }

            return ((IDictionary<string, object>) requestOptions).ContainsKey(key);
        }

        /// <summary>
        ///     Add key value pair to the <see cref="HttpRequestOptions"/>
        /// </summary>
        /// <typeparam name="TValue">The type of the value that is being added</typeparam>
        /// <param name="requestOptions"><see cref="HttpRequestOptions"/></param>
        /// <param name="key">The key corresponding to the value that is being added</param>
        /// <param name="value">The value that is being added to the <see cref="HttpRequestOptions"/></param>
        /// <param name="overwrite">Determines if the existing should be overwritten or ignored by adding operation.</param>
        /// <returns>True when the key value pair has been successfully added, otherwise false</returns>
        public static bool Add<TValue>(
            this HttpRequestOptions requestOptions,
            string key,
            TValue value,
            bool overwrite = true)
        {
            if (requestOptions.ContainsKey(key) && overwrite)
            {
                requestOptions.Remove(key, out _);
                return requestOptions.TryAdd(key, value);
            }

            return requestOptions.TryAdd(key, value);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="requestOptions"><see cref="HttpRequestOptions"/></param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key" /> is <see langword="null" />.</exception>
        /// <returns>
        /// <see langword="true" /> if the object that implements <see cref="IDictionary{TKey,TValue}" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public static bool TryGetValue(
            this HttpRequestOptions requestOptions,
            string key,
            [MaybeNullWhen(false)] out object value)
        {
            return ((IDictionary<string, object>) requestOptions).TryGetValue(key, out value);
        }
    }
}
