using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when assignments of objects are updated.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class ObjectAssignmentEvent : DomainEventBaseV2<AssignmentPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionCreatedEvent" />.
    /// </summary>
    public ObjectAssignmentEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ObjectAssignmentEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data of batch request as a <see cref="AssignmentPayload" />.</param>
    public ObjectAssignmentEvent(DateTime timestamp, AssignmentPayload payload) : base(timestamp, payload)
    {
    }
}
