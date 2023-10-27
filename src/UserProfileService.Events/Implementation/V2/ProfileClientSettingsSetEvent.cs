using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when client settings are set for a profile (user,group or organization).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileClientSettingsSetEvent : DomainEventBaseV2<ClientSettingsSetPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetPayload" />.
    /// </summary>
    public ProfileClientSettingsSetEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetPayload" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies all <see cref="ClientSettingsSetPayload" /> which have been set.</param>
    public ProfileClientSettingsSetEvent(DateTime timestamp, ClientSettingsSetPayload payload) :
        base(timestamp, payload)
    {
    }
}
