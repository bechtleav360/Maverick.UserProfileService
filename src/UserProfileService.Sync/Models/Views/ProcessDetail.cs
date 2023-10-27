using System;
using System.Collections.Generic;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Models.Views;

/// <summary>
///     Describes a view of the sync process.
/// </summary>
public class ProcessDetail
{
    /// <summary>
    ///     Timestamp when synchronization finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    ///     Initiator who triggered the process
    /// </summary>
    public ActionInitiator Initiator { get; set; }

    /// <summary>
    ///     Timestamp when synchronization started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    ///     Status of process.
    /// </summary>
    public ProcessStatus Status { get; set; } = ProcessStatus.Initialize;

    /// <summary>
    ///     List of all information about system synchronizations.
    /// </summary>
    public IDictionary<string, SystemView> Systems { get; set; } = new Dictionary<string, SystemView>();

    /// <summary>
    ///     Timestamp when synchronization state changed.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
