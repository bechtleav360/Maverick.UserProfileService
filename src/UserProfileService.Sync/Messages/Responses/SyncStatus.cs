using UserProfileService.Messaging.Annotations;
using UserProfileService.Sync.Models;

namespace UserProfileService.Sync.Messages.Responses;

/// <summary>
///     Get the status of the sync process
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SyncStatus
{
    /// <summary>
    ///     True when a sync process is running, otherwise false
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public bool IsRunning { get; set; }

    /// <summary>
    ///     Contains information about the running sync process (will be null, when no process is running)
    /// </summary>
    public ProcessView Process { get; set; }

    /// <summary>
    ///     Identifier of the status request (optional)
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public string RequestId { get; set; }
}
