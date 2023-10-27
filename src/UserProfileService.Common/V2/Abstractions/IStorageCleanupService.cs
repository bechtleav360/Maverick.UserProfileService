using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Contains methods to do a cleanup in the estimated storage service.
/// </summary>
public interface IStorageCleanupService
{
    /// <summary>
    ///     The technique/platform that is supported by this implementation of <see cref="IStorageCleanupService" />.
    /// </summary>
    string RelevantFor { get; }

    /// <summary>
    ///     Starts a cleanup operation.
    /// </summary>
    /// <param name="argument">Optional argument object.</param>
    /// <param name="cancellationToken">The cancellation token that is used to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CleanupAll(object argument = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a cleanup operation (only first-level projection data).
    /// </summary>
    /// <param name="argument">Optional argument object.</param>
    /// <param name="cancellationToken">The cancellation token that is used to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CleanupMainProjectionDataAsync(
        object argument = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a cleanup operation (only second-level projection data).
    /// </summary>
    /// <param name="argument">Optional argument object.</param>
    /// <param name="cancellationToken">The cancellation token that is used to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CleanupExtendedProjectionDataAsync(
        object argument = null,
        CancellationToken cancellationToken = default);
}
