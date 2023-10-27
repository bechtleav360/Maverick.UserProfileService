using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages.Commands;

/// <summary>
///     Defines a command for the start of the sync.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class StartSyncCommand
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    ///     The identifier of the command initiator.
    /// </summary>
    public string InitiatorId { get; set; }

    /// <summary>
    ///     True if the command has been started by the scheduler, otherwise false
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public bool StartedByScheduler { get; set; }
}
