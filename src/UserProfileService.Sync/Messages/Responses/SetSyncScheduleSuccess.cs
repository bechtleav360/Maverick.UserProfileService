using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages.Responses;

/// <summary>
///     Defines a success message after the sync schedule status has been successfully set.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SetSyncScheduleSuccess
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public Guid? CorrelationId { get; set; }
}
