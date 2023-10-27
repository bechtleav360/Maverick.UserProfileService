using System;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;

/// <summary>
///     Contains methods to log performance messages of AQL queries.
/// </summary>
internal interface IPerformanceLogger
{
    /// <summary>
    ///     Logs performance data of a cursor request.
    /// </summary>
    /// <param name="request">The original request to the cursor API.</param>
    /// <param name="response">The response from ArangoDb service.</param>
    /// <param name="executionTime">The measured execution time.</param>
    /// <param name="timestamp">The time stamp when the execution started. </param>
    /// <param name="transactionId">The id of the transaction the query related to.</param>
    /// <returns></returns>
    Task LogAsync(
        CreateCursorBody request,
        ICursorInnerResponse response,
        TimeSpan executionTime,
        DateTime timestamp,
        string transactionId = null);
}
