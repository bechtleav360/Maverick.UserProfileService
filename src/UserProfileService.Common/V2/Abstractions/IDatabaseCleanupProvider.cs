using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Identifies a cleanup service for a database that will be triggered by a background service.
/// </summary>
public interface IDatabaseCleanupProvider
{
    /// <summary>
    ///     Do an asynchronous cleanup operation.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CleanupAsync(
        CancellationToken cancellationToken = default);
}
