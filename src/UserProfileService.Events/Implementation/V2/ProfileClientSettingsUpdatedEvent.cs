using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when client settings are updated (merge) for a profile (user,group or organization).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileClientSettingsUpdatedEvent : DomainEventBaseV2<ClientSettingsSetPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetPayload" />.
    /// </summary>
    public ProfileClientSettingsUpdatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetPayload" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies all <see cref="ClientSettingsSetPayload" /> which have been set.</param>
    public ProfileClientSettingsUpdatedEvent(DateTime timestamp, ClientSettingsSetPayload payload) :
        base(timestamp, payload)
    {
    }
}
