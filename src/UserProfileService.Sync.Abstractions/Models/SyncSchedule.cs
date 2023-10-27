using System;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Defines the current schedule of tasks of the sync.
/// </summary>
public class SyncSchedule
{
    /// <summary>
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// </summary>
    public string ModifiedBy { get; set; }
}
