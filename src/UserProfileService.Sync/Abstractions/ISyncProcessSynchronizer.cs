using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Messages.Commands;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Contains some methods used to synchronize sync processes.
/// </summary>
public interface ISyncProcessSynchronizer
{
    /// <summary>
    ///     Return true if the sync lock object is available and the sync can be started otherwise false
    /// </summary>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    Task<bool> IsSyncLockAvailableAsync(CancellationToken token = default);

    /// <summary>
    ///     Method used to check if the syn can be started according to the defined rules.
    /// </summary>
    /// <param name="startSyncCommand"></param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns> True if the sync can be started, otherwise false</returns>
    Task<bool> TryStartSync(StartSyncCommand startSyncCommand, CancellationToken token = default);

    /// <summary>
    ///     Release the lock for the running process.
    /// </summary>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns> True if the sync can be complete, otherwise false</returns>
    Task<bool> ReleaseLockForRunningProcessAsync(CancellationToken token = default);
}
