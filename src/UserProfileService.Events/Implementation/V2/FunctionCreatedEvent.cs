using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a new function was created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class FunctionCreatedEvent : DomainEventBaseV2<FunctionCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionCreatedEvent" />.
    /// </summary>
    public FunctionCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data of the created function as a <see cref="FunctionCreatedPayload" />.</param>
    public FunctionCreatedEvent(DateTime timestamp, FunctionCreatedPayload payload) : base(timestamp, payload)
    {
    }
}
