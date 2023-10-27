using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

/// <summary>
///     Defines a target for ConfigCommand conversions.
/// </summary>
public interface ITarget : IAsyncDisposable
{
    /// <summary>
    ///     Completes the write operation.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CompleteAsync(CancellationToken ct = default);

    /// <summary>
    ///     Gets the stream to be used to accept write requests.
    /// </summary>
    /// <returns>The stream to be used.</returns>
    Stream GetStream();
}
