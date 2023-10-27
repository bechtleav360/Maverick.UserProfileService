using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.States.Messages;

/// <summary>
///     This message is emitted when the synchronization process is to be aborted.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class AbortSyncMessage
{
    /// <summary>
    ///     The correlation Id of the sync process that should be aborted.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public Guid Id { get; set; }
}
