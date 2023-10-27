using System;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when properties in a function were updated.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class FunctionPropertiesChangedEvent : DomainEventBaseV2<PropertiesUpdatedPayload>
{
    /// <summary>
    ///     Function before changing the properties.
    /// </summary>
    public FunctionView OldFunction { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionPropertiesChangedEvent" />.
    /// </summary>
    public FunctionPropertiesChangedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="FunctionPropertiesChangedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Contains the function and the modified properties <see cref="PropertiesUpdatedPayload" />.</param>
    public FunctionPropertiesChangedEvent(DateTime timestamp, PropertiesUpdatedPayload payload) : base(
        timestamp,
        payload)
    {
    }
}
