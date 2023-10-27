using System;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Transaction object for the estimated cache implementation.
/// </summary>
public interface ICacheTransaction : IAsyncDisposable, IDisposable
{
    /// <summary>
    ///     The payload of this transaction.
    /// </summary>
    object Payload { get; }

    /// <summary>
    ///     Commits the transaction, sends all commands to the server as a batch.
    /// </summary>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that represents the asynchronous write/read operation.</returns>
    Task CommitAsync(CancellationToken token = default);
}
