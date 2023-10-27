using System.Collections.Generic;
using System.Threading;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Represents an arango transaction.
/// </summary>
public class ArangoTransaction : IDatabaseTransaction
{
    /// <summary>
    ///     Offers locking functionality to avoid concurrent requests with the same collection.
    ///     See ArangoDbs docs about streamTransactions for more info about simultaneous requests to stream transactions.
    /// </summary>
    public readonly SemaphoreSlim TransactionLock = new SemaphoreSlim(1, 1);

    /// <inheritdoc />
    public CallingServiceContext CallingService { get; set; }

    /// <summary>
    ///     Lists all collections which are involved in the transaction.
    /// </summary>
    public IList<string> Collections { get; set; }

    /// <summary>
    ///     State whether the transaction is active or not.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    ///     Specifies the id of the transaction.
    /// </summary>
    public string TransactionId { get; set; }

    /// <summary>
    ///     Invalidates the current transaction.
    /// </summary>
    internal void MarkAsInactive()
    {
        IsActive = false;
    }
}
