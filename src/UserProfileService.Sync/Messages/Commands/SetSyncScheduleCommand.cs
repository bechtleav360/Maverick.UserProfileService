using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages.Commands;

/// <summary>
///     Defines a command for the ups saga worker.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SetSyncScheduleCommand
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
    ///     True when the sync schedule enabled otherwise false.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public bool? IsActive { get; set; }
}
