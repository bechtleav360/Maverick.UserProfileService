using System;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V3;

/// <summary>
///     This event is emitted when a new user setting section is created.
///     BE AWARE, the version is related to the Event and not to the API.
/// </summary>
public class UserSettingsSectionCreatedEvent : DomainEventBaseV3<UserSettingSectionCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="UserSettingsSectionCreatedEvent" />.
    /// </summary>
    public UserSettingsSectionCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="UserSettingsSectionCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Data of the created user wrapped in a <see cref="UserCreatedPayload" />.</param>
    public UserSettingsSectionCreatedEvent(DateTime timestamp, UserSettingSectionCreatedPayload payload) : base(
        timestamp,
        payload)
    {
    }
}
