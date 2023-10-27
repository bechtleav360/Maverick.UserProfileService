using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages.Responses;

/// <summary>
///     The message that is being sent from the event collector after received an event collecting request.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class StartCollectingEventSuccess
{
    /// <summary>
    ///     The id to be used to collect messages and for which a common response is to be sent.
    /// </summary>
    public Guid? CollectingId { get; set; }

    /// <summary>
    ///     The id of the sender who sent the command to uniquely assign the response.
    ///     Not to be confused with the correlation id,
    ///     which is sent in the header of the message and is passed along by the individual services.
    /// </summary>
    public string ExternalProcessId { get; set; }
}
