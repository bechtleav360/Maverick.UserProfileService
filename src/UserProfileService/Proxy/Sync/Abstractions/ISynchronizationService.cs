using UserProfileService.Common.V2.Utilities;
using UserProfileService.Proxy.Sync.Models;
using UserProfileService.Proxy.Sync.Utilities;

namespace UserProfileService.Proxy.Sync.Abstractions;

/// <summary>
///     Defines the service to handle synchronization processes.
/// </summary>
public interface ISynchronizationService
{
    /// <summary>
    ///     Starts a new synchronization process
    /// </summary>
    /// <param name="correlationId">The correlationId for the synchronization run.</param>
    /// <param name="schedule">Indicates if the sync process is initiated by scheduler.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>The identifier of the new synchronization process.</returns>
    public Task<Guid> StartSynchronizationAsync(
        string correlationId,
        bool schedule,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Return the sync process for the given id.
    /// </summary>
    /// <param name="id">Id of process to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>A process as of <see cref="ProcessView" />.</returns>
    public Task<ProcessView> GetProcessAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return a detailed sync process for the given id.
    /// </summary>
    /// <param name="id">Id of process to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>A process as of <see cref="ProcessDetail" />.</returns>
    public Task<ProcessDetail> GetDetailedProcessAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return the list of running and finished sync processes.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>A paginated list of <see cref="ProcessView" />.</returns>
    public Task<PaginatedListResponse<ProcessView>> GetAllProcessesAsync(
        QueryObject queryObject,
        CancellationToken cancellationToken = default);
}
