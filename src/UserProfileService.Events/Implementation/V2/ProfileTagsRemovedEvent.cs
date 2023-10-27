using System;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when tags are removed from a profile (user or group).
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileTagsRemovedEvent : DomainEventBaseV2<TagsRemovedPayload>
{
    /// <summary>
    ///     Type of entity to remove the tags from.
    /// </summary>
    public ProfileKind ProfileKind { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileTagsRemovedEvent" />.
    /// </summary>
    public ProfileTagsRemovedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileTagsRemovedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">A <see cref="TagsRemovedPayload" /> containing the deleted tags.</param>
    public ProfileTagsRemovedEvent(DateTime timestamp, TagsRemovedPayload payload) : base(timestamp, payload)
    {
    }
}
