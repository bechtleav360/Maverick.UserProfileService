using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is raised when tags are set for a Role.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class RoleTagsAddedEvent : DomainEventBaseV2<TagsSetPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RoleTagsAddedEvent" />.
    /// </summary>
    public RoleTagsAddedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RoleTagsAddedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies all Tags as a <see cref="TagsSetPayload" /> which have been set.</param>
    public RoleTagsAddedEvent(DateTime timestamp, TagsSetPayload payload) : base(timestamp, payload)
    {
    }
}
