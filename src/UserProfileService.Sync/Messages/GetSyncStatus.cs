using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages;

/// <summary>
///     Defines a message used to request current sync status.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class GetSyncStatus
{
    /// <summary>
    ///     Request identifier for getting status request (can be used to assign request to response message)
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public string RequestId { get; set; }
}
