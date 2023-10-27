using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a new tag is created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class TagCreatedEvent : DomainEventBaseV2<TagCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="TagCreatedEvent" />.
    /// </summary>
    public TagCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="TagCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Data of the created user wrapped in a <see cref="TagCreatedPayload" />.</param>
    public TagCreatedEvent(DateTime timestamp, TagCreatedPayload payload) : base(timestamp, payload)
    {
    }
}
