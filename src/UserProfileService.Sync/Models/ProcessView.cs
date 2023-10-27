using System;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Models;

/// <summary>
///     Describes a sync process with some meta data.
/// </summary>
public class ProcessView
{
    /// <summary>
    ///     Internal correlation id of saga and is equal to sync process id.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    ///     End date of the sync process
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    ///     The initiator of the sync process <see cref="ActionInitiator" />.
    /// </summary>
    public ActionInitiator Initiator { get; set; }

    /// <summary>
    ///     Last activity date in the sync process
    /// </summary>
    public DateTime? LastActivity { get; set; }

    /// <summary>
    ///     Start date of the sync process
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    ///     Status of the sync process <see cref="ProcessStatus" />.
    /// </summary>
    public ProcessStatus Status { get; set; }

    /// <summary>
    ///     Amount of operations executed for each entity types during the sync process.
    /// </summary>
    public Operations SyncOperations { get; set; }
}
