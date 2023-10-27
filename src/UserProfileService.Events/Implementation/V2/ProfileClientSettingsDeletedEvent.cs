using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when client settings are deleted from a profile (user, group or organization).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileClientSettingsDeletedEvent : DomainEventBaseV2<ClientSettingsDeletedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsDeletedPayload" />.
    /// </summary>
    public ProfileClientSettingsDeletedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsDeletedPayload" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">A <see cref="ClientSettingsDeletedPayload" /> containing the properties to delete.</param>
    public ProfileClientSettingsDeletedEvent(DateTime timestamp, ClientSettingsDeletedPayload payload) :
        base(timestamp, payload)
    {
    }
}
