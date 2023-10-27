using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages.Responses;

/// <summary>
///     Get the status of the sync scheduler
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SyncScheduleStatus
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    ///     True when the sync schedule enabled otherwise false.
    /// </summary>
    public bool? IsActive { get; set; }
}
