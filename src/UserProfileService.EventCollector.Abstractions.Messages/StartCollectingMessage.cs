using System;
using UserProfileService.Messaging;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.EventCollector.Abstractions.Messages;

/// <summary>
///     The message that is being sent to trigger the event collector agent.
/// </summary>
[Message(ServiceName = "sync", ServiceGroup = "user-profile")]
public class StartCollectingMessage : IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The number of items (responses) that should be collected.
    ///     Sometimes this value is not known, in this case the variable is null.
    /// </summary>
    public int? CollectItemsAccount { get; set; }

    /// <summary>
    ///     Defines when a status should be sent out for the already collected responses for this entity.
    /// </summary>
    public StatusDispatch Dispatch { get; set; } = new StatusDispatch(100);

    /// <summary>
    ///     The id of the external process that triggered the event.
    /// </summary>
    public string ExternalProcessId { get; set; }
}
