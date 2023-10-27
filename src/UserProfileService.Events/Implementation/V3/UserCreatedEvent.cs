using System;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V3;

/// <summary>
///     This event is emitted when a new user is created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class UserCreatedEvent : DomainEventBaseV3<UserCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="UserCreatedEvent" />.
    /// </summary>
    public UserCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="UserCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Data of the created user wrapped in a <see cref="UserCreatedPayload" />.</param>
    public UserCreatedEvent(DateTime timestamp, UserCreatedPayload payload) : base(timestamp, payload)
    {
    }
}
