using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when client settings are set for profiles (user,group or organization).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileClientSettingsSetBatchEvent : DomainEventBaseV2<ClientSettingsSetBatchPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetBatchPayload" />.
    /// </summary>
    public ProfileClientSettingsSetBatchEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ClientSettingsSetBatchPayload" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies all <see cref="ClientSettingsSetBatchPayload" /> which have been set.</param>
    public ProfileClientSettingsSetBatchEvent(DateTime timestamp, ClientSettingsSetBatchPayload payload) :
        base(timestamp, payload)
    {
    }
}
