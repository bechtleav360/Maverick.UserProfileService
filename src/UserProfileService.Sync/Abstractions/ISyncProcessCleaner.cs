using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Contains some methods used to clear hanging sync processes
/// </summary>
public interface ISyncProcessCleaner
{
    /// <summary>
    ///     Clear status of hanging processes. (state will be set to aborted in the database)
    /// </summary>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns> A <see cref="Task" /></returns>
    Task UpdateAbortedProcessesStatusAsync(CancellationToken token = default);
}
