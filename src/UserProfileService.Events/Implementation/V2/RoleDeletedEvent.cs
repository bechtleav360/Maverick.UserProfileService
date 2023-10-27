using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a role has been deleted.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class RoleDeletedEvent : DomainEventBaseV2<IdentifierPayload>
{
    /// <summary>
    ///     Role before deletion.
    /// </summary>
    public RoleBasic OldRole { get; set; }

    /// <summary>
    ///     Assigned profiles of role.
    /// </summary>
    public IEnumerable<Member> Profiles { get; set; } = new List<Member>();

    /// <summary>
    ///     Initializes a new instance of <see cref="RoleDeletedEvent" />.
    /// </summary>
    public RoleDeletedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="RoleDeletedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the id of the deleted role.</param>
    public RoleDeletedEvent(DateTime timestamp, IdentifierPayload payload) : base(timestamp, payload)
    {
    }
}
