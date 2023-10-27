using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;

namespace UserProfileService.Proxy.Sync.Abstractions;

/// <summary>
///     Defines the service to handle schedule of synchronization processes.
/// </summary>
public interface IScheduleService
{
    /// <summary>
    ///     Get the current schedule of synchronization process.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    public Task<SyncSchedule> GetScheduleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Change the schedule of synchronization process.
    /// </summary>
    /// <param name="schedule">Request to change schedule with.</param>
    /// <param name="userId">Identifier of the user who executed the request.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    public Task<SyncSchedule> ChangeScheduleAsync(
        ScheduleRequest schedule,
        string userId = null,
        CancellationToken cancellationToken = default);
}
