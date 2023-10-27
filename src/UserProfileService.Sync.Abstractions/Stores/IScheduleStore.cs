using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstraction.Stores;

/// <summary>
///     Describe the store to handle schedule of synchronization process.
/// </summary>
public interface IScheduleStore
{
    /// <summary>
    ///     Get the schedule of synchronization process
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    public Task<SyncSchedule> GetScheduleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Save the schedule of synchronization process.
    /// </summary>
    /// <param name="schedule">Request to change schedule with.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Current state of sync schedule.</returns>
    public Task<SyncSchedule> SaveScheduleAsync(
        SyncSchedule schedule,
        CancellationToken cancellationToken = default);
}
