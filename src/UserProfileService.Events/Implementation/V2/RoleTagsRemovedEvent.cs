using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when tags are removed from a Role.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class RoleTagsRemovedEvent : DomainEventBaseV2<TagsRemovedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RoleTagsRemovedEvent" />.
    /// </summary>
    public RoleTagsRemovedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RoleTagsRemovedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">A <see cref="TagsRemovedPayload" /> containing the deleted tags.</param>
    public RoleTagsRemovedEvent(DateTime timestamp, TagsRemovedPayload payload) : base(timestamp, payload)
    {
    }
}
