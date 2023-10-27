using System;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a profile has been deleted.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ProfileDeletedEvent : DomainEventBaseV2<ProfileIdentifierPayload>
{
    /// <summary>
    ///     A list of all assignments that belong to this profile.
    ///     e.g: Users of a group.
    ///     Note: For users the collection will always be empty.
    /// </summary>
    public ProfileIdent[] Children { get; set; } = Array.Empty<ProfileIdent>();

    /// <summary>
    ///     Profile before deletion.
    /// </summary>
    public IProfile OldProfile { get; set; }

    /// <summary>
    ///     A list of all assignments that belong to this profile.
    ///     e.g: Organization of a user.
    /// </summary>
    public ProfileIdent[] Parents { get; set; } = Array.Empty<ProfileIdent>();

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileDeletedEvent" />.
    /// </summary>
    public ProfileDeletedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProfileDeletedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the id of the deleted profile.</param>
    public ProfileDeletedEvent(DateTime timestamp, ProfileIdentifierPayload payload) : base(timestamp, payload)
    {
    }
}
