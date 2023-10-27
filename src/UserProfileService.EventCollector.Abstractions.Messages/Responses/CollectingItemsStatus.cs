using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages.Responses;

/// <summary>
///     Describes the status of the event collector for a process that has already been started.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class CollectingItemsStatus : IEventCollectorMessage
{
    /// <summary>
    ///     The number of items (responses) that have been already collected by the event collector agent.
    /// </summary>
    public int CollectedItemsAccount { get; set; }

    /// <summary>
    ///     The id to be used to collect messages and for which a common response is to be sent.
    /// </summary>
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The id of the external process that triggered the event.
    /// </summary>
    public string ExternalProcessId { get; set; }
}
