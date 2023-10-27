using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a new group is created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class GroupCreatedEvent : DomainEventBaseV2<GroupCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="GroupCreatedEvent" />.
    /// </summary>
    public GroupCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="GroupCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data of the created group as a <see cref="GroupCreatedPayload" />.</param>
    public GroupCreatedEvent(DateTime timestamp, GroupCreatedPayload payload) : base(timestamp, payload)
    {
    }
}
