using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Protocol.Extensions;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Contains static methods to extend <see cref="HttpRequestMessage" /> instances.
/// </summary>
internal static class HttpRequestExtensions
{
    private const string TimeoutPropertyKey = "RequestTimeout";
    private const string TransactionInfoPropertyKey = "TransactionInformation";

    /// <summary>
    ///     Sets the timeout property of the <paramref name="request" />.
    /// </summary>
    /// <param name="request">The request that should be modified.</param>
    /// <param name="timeout">A <see cref="TimeSpan" /> within a request must be executed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request" /> is <c>null</c>.</exception>
    internal static void SetTimeout(
        this HttpRequestMessage request,
        TimeSpan? timeout)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Options.Add(TimeoutPropertyKey, timeout);
    }

    /// <summary>
    ///     Returns the value of the timeout property of the <paramref name="request" />.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The timeout value stored in the <paramref name="request" />. Or <c>null</c>, if none has been found.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="request" /> is <c>null</c>.</exception>
    internal static TimeSpan? GetTimeout(this HttpRequestMessage request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Options.TryGetValue(
                TimeoutPropertyKey,
                out object value)
            && value is TimeSpan timeout)
        {
            return timeout;
        }

        return null;
    }

    /// <summary>
    ///     Extracts the transaction id (if present) from request header.
    /// </summary>
    /// <param name="request">The request that contains the transaction id.</param>
    /// <returns>The transaction id as string, if found. Otherwise <c>null</c>.</returns>
    internal static string ExtractTransactionId(this HttpRequestMessage request)
    {
        if (request == null)
        {
            return null;
        }

        string transactionId =
            request.Headers.TryGetValues(ParameterName.TransactionId, out IEnumerable<string> values)
                ? values?.FirstOrDefault()
                : null;

        return !string.IsNullOrWhiteSpace(transactionId) ? transactionId : null;
    }

    /// <summary>
    ///     Stores the transaction information as string property of the <paramref name="request" />. This can be used for
    ///     debugging.
    /// </summary>
    /// <param name="request">The request to be modified.</param>
    /// <param name="transactionInformation">Information about the transaction (i.e. write/read collections).</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request" /> is <c>null</c>.</exception>
    internal static void StoreTransactionInformation(
        this HttpRequestMessage request,
        string transactionInformation)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Options.Add(TransactionInfoPropertyKey, transactionInformation);
    }

    /// <summary>
    ///     Returns the value of the timeout property of the <paramref name="request" />.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The transaction information stored in the <paramref name="request" />. Or <c>null</c>, if none has been found.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="request" /> is <c>null</c>.</exception>
    internal static string GetTransactionInformation(this HttpRequestMessage request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Options.TryGetValue(
                TransactionInfoPropertyKey,
                out object value)
            && value is string info)
        {
            return info;
        }

        return null;
    }
}
