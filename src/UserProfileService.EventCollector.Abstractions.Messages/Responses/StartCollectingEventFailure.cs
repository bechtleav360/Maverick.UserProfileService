using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages.Responses;

/// <summary>
///     The message that is being sent from the event collector when the received collecting event request failed.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class StartCollectingEventFailure
{
    /// <summary>
    ///     The correlation id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    public Guid? CollectingId { get; set; }

    /// <summary>
    ///     Describes the occurred error.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     The id of the external process that triggered the event.
    /// </summary>
    public string ExternalProcessId { get; set; }
}
