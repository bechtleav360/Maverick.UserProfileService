using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Sync.Messages.Responses;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.Views;

namespace UserProfileService.Sync.Abstractions;

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
    ///     Return the status of the running UPS-Sync instance.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <param name="requestId">The identifier of the request</param>
    /// <returns> The status of the actual UPS-Sync instance as <see cref="SyncStatus" /></returns>
    public Task<SyncStatus> GetSyncStatusAsync(string requestId = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns current information about running process (if any synchronization is running).
    /// </summary>
    /// <remarks> Will return null if no synchronization is running </remarks>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns> Current running sync process list as <see cref="ProcessView" /> </returns>
    public Task<IEnumerable<ProcessView>> GetRunningSyncProcessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return the list of running and finished sync processes.
    /// </summary>
    /// <param name="page">Number of page to retrieve.</param>
    /// <param name="pageSize">Size of page to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>A paginated list of <see cref="ProcessView" />.</returns>
    public Task<PaginatedList<ProcessView>> GetAllProcessesAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return the list of running and finished sync processes.
    /// </summary>
    /// <param name="queryObject">Includes filter, sorting and pagination settings.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>A paginated list of <see cref="ProcessView" />.</returns>
    public Task<PaginatedList<ProcessView>> GetAllProcessesAsync(
        QueryObject queryObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update status of the sync processes which has been aborted or not running anymore
    /// </summary>
    /// <param name="processId">The process identifier</param>
    /// <param name="token">Propagates notification that operations should be canceled</param>
    /// <returns>A Task <see cref="Task" /></returns>
    public Task DeclareProcessAbortedAsync(Guid processId, CancellationToken token = default);
}
