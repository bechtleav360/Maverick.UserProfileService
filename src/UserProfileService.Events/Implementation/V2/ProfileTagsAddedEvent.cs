using System;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when tags are set for a profile (user or group).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileTagsAddedEvent : DomainEventBaseV2<TagsSetPayload>
{
    /// <summary>
    ///     Type of entity to add the tags to.
    /// </summary>
    public ProfileKind ProfileKind { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileTagsAddedEvent" />.
    /// </summary>
    public ProfileTagsAddedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileTagsAddedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies all Tags as a <see cref="TagsSetPayload" /> which have been set.</param>
    public ProfileTagsAddedEvent(DateTime timestamp, TagsSetPayload payload) : base(timestamp, payload)
    {
    }
}
