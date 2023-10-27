using System;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V3;

/// <summary>
///     This event is emitted when a new user setting section is created.
///     BE AWARE, the version is related to the Event and not to the API.
/// </summary>
public class UserSettingObjectUpdatedEvent : DomainEventBaseV3<UserSettingObjectUpdatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="UserSettingObjectUpdatedEvent" />.
    /// </summary>
    public UserSettingObjectUpdatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="UserSettingObjectUpdatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Data of the created user wrapped in a <see cref="UserCreatedPayload" />.</param>
    public UserSettingObjectUpdatedEvent(DateTime timestamp, UserSettingObjectUpdatedPayload payload) : base(
        timestamp,
        payload)
    {
    }
}
