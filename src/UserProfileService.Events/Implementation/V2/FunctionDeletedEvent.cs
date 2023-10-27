using System;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a function has been deleted.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class FunctionDeletedEvent : DomainEventBaseV2<IdentifierPayload>
{
    /// <summary>
    ///     Function before deletion.
    /// </summary>
    public FunctionView OldFunction { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionDeletedEvent" />.
    /// </summary>
    public FunctionDeletedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionDeletedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the id of the deleted function.</param>
    public FunctionDeletedEvent(DateTime timestamp, IdentifierPayload payload) : base(timestamp, payload)
    {
    }
}
