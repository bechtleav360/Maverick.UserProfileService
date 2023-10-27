using System;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a tag has been deleted.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class TagDeletedEvent : DomainEventBaseV2<IdentifierPayload>
{
    /// <summary>
    ///     Tag before deletion.
    /// </summary>
    public Tag OldTag { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="TagDeletedEvent" />.
    /// </summary>
    public TagDeletedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="TagDeletedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the id of the deleted function.</param>
    public TagDeletedEvent(DateTime timestamp, IdentifierPayload payload) : base(timestamp, payload)
    {
    }
}
