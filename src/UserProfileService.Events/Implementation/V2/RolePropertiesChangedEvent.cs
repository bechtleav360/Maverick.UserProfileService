using System;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when properties in a role were updated.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class RolePropertiesChangedEvent : DomainEventBaseV2<PropertiesUpdatedPayload>
{
    /// <summary>
    ///     Role before changing properties.
    /// </summary>
    public RoleBasic OldRole { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="RolePropertiesChangedEvent" />.
    /// </summary>
    public RolePropertiesChangedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RolePropertiesChangedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data as <see cref="PropertiesUpdatedPayload" /> which is contained in this event.</param>
    public RolePropertiesChangedEvent(DateTime timestamp, PropertiesUpdatedPayload payload) :
        base(timestamp, payload)
    {
    }
}
