using System;

namespace UserProfileService.EventCollector.Abstractions.Messages;

/// <summary>
///     Defines a message for the event collector agent.
/// </summary>
public interface IEventCollectorMessage
{
    /// <summary>
    ///     The id to be used to collect messages and for which a common response should be sent.
    /// </summary>
    public Guid CollectingId { get; }
}
