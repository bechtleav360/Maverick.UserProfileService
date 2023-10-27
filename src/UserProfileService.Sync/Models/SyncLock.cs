using System;

namespace UserProfileService.Sync.Models;

/// <summary>
///     Object used to synchronize sync processes.
/// </summary>
public class SyncLock
{
    /// <summary>
    ///     True if the lock is released otherwise false
    /// </summary>
    public bool IsReleased { get; set; }

    /// <summary>
    ///     Update time of the lock.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public DateTime UpdatedAt { get; set; }
}
