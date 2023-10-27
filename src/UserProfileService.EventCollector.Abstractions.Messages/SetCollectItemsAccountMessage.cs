using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages;

/// <summary>
///     The message that is being sent to actualize the number of event collector agent.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class SetCollectItemsAccountMessage : IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The number of items (responses) that should be collected.
    /// </summary>
    public int CollectItemsAccount { get; set; }
}
