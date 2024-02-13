using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     Represents a repository of the projection state and contains methods to read/write
///     data related to the projection state.
/// </summary>
public interface IProjectionStateRepository
{
    /// <summary>
    ///     Gets the numbers of the latest events per stream previously projected by a handler.
    /// </summary>
    /// <param name="stoppingToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a dictionary containing all latest events per
    ///     streams.
    /// </returns>
    Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default);

    /// <summary>
    ///     Saves the projection state in the database.
    /// </summary>
    /// <param name="projectionState">The state of the projection to be stored.</param>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task SaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the information about the global position of the latest projected event.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the information about the global position.</returns>
    Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default);
}
