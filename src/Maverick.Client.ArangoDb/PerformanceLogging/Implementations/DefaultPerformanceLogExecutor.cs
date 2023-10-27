using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.ExternalLibraries.DependencyInjection;
using Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.PerformanceLogging.Implementations;

/// <summary>
///     Contains methods to measure performance of queries sent to ArangoDb.
/// </summary>
internal static class DefaultPerformanceLogExecutor
{
    /// <summary>
    ///     Measures the execution time of cursor requests and logs result to <see cref="IPerformanceLogger" />.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="method">The method that is used to get the response from ArangoDb.</param>
    /// <param name="cursorBody">The body of the request to create a cursor.</param>
    /// <param name="transactionId">The id of the transaction the request belongs to.</param>
    /// <returns>
    ///     A task representing the asynchronous read/write operation. It contains the response object retrieved by
    ///     <paramref name="method" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="method" /> is <c>null</c>.<br />
    ///     -or-<br />
    ///     <paramref name="cursorBody" /> is <c>null</c>.
    /// </exception>
    internal static async Task<TResponse> LogPerformanceAsync<TResponse>(
        Func<Task<TResponse>> method,
        CreateCursorBody cursorBody,
        string transactionId = null)
        where TResponse : ICursorResponse
    {
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        if (cursorBody == null)
        {
            throw new ArgumentNullException(nameof(cursorBody));
        }

        var logger = Ioc.Default.GetService<IPerformanceLogger>();

        if (logger == null)
        {
            return await method.Invoke().ConfigureAwait(false);
        }

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        DateTime started = DateTime.UtcNow;
        TResponse response = await method.Invoke().ConfigureAwait(false);
        stopWatch.Stop();

        try
        {
            await logger.LogAsync(cursorBody, response?.CursorDetails, stopWatch.Elapsed, started, transactionId);
        }
        catch
        {
            // ignored
        }

        return response;
    }
}
