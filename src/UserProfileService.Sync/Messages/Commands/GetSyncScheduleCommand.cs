using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages.Commands;

/// <summary>
///     Defines a command to get the status of the sync scheduler.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class GetSyncScheduleCommand
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public Guid? CorrelationId { get; set; }

    /// <summary>
    ///     True when the sync schedule enabled otherwise false
    ///     (when the schedule is enabled, the synchronization will run in a defined time interval).
    /// </summary>
    public bool? IsActive { get; set; } = null;
}
