using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Contains methods to prepare data storage like databases.
/// </summary>
public interface IDatabasePreparationService
{
    /// <summary>
    ///     The technique/platform that is supported by this implementation of <see cref="IDatabasePreparationService" />.
    /// </summary>
    string RelevantFor { get; }

    /// <summary>
    ///     Starts a preparation operation on the targeted storage provider / database system.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that is used to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task PrepareAsync(CancellationToken cancellationToken = default);
}
