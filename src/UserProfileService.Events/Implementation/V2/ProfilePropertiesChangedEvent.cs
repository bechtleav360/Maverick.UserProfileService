using System;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when properties in a profile were updated.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfilePropertiesChangedEvent : DomainEventBaseV2<PropertiesUpdatedPayload>
{
    /// <summary>
    ///     Profile before changing properties.
    /// </summary>
    public IProfile OldProfile { get; set; }

    /// <summary>
    ///     Type of entity to add the tags to.
    /// </summary>
    public ProfileKind ProfileKind { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfilePropertiesChangedEvent" />.
    /// </summary>
    public ProfilePropertiesChangedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfilePropertiesChangedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">
    ///     Contains the updated properties and the related profile wrapped in a
    ///     <see cref="PropertiesUpdatedPayload" />.
    /// </param>
    public ProfilePropertiesChangedEvent(DateTime timestamp, PropertiesUpdatedPayload payload) :
        base(timestamp, payload)
    {
    }
}
