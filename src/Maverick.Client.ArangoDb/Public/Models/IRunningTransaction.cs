using System;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public.Models.Transaction;

namespace Maverick.Client.ArangoDb.Public.Models;

/// <summary>
///     Contains members of an active (or already finished) transaction.
/// </summary>
public interface IRunningTransaction : IAsyncDisposable, IDisposable
{
    internal Connection UsedConnection { get; }

    /// <summary>
    ///     Contains information about an API error occurred during creation of the transaction.
    /// </summary>
    Exception Exception { get; }

    /// <summary>
    ///     Gets the status of the transaction (meaning if it is still running or finished (either committed or aborted)).
    /// </summary>
    /// <returns>The status as enumeration.</returns>
    TransactionStatus GetTransactionStatus();

    /// <summary>
    ///     Gets the id of this transaction instance.
    /// </summary>
    /// <returns>The id of the transaction.</returns>
    string GetTransactionId();
}
