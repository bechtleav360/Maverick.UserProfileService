using System;

namespace UserProfileService.EventCollector.Abstractions.Messages;

/// <summary>
///     The message that is being sent to get the status of specified collecting process in the event collector agent.
/// </summary>
public class GetCollectingItemsStatusMessage : IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }
}
