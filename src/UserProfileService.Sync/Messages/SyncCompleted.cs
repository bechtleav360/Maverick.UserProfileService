using System;
using UserProfileService.Messaging.Annotations;
using UserProfileService.Sync.Models;

namespace UserProfileService.Sync.Messages;

/// <summary>
///     Defines a message send when a synchronization has been completed
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SyncCompleted
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public Guid? CorrelationId { get; set; }

    /// <summary>
    ///     Identifier of initiator who triggered the process
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public ActionInitiator Initiator { get; set; }

    /// <summary>
    ///     Contains further information about the completed sync process.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public ProcessView Process { get; set; }
}
