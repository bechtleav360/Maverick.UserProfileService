using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a new role is created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class RoleCreatedEvent : DomainEventBaseV2<RoleCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RoleCreatedEvent" />.
    /// </summary>
    public RoleCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RoleCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data as <see cref="RoleCreatedPayload" /> which is contained in this event.</param>
    public RoleCreatedEvent(DateTime timestamp, RoleCreatedPayload payload) : base(timestamp, payload)
    {
    }
}
