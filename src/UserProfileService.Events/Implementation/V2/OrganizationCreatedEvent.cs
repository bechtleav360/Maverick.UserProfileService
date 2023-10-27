using System;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Events.Implementation.V2;

/// <summary>
///     This event is emitted when a new organization is created.
///     BE AWARE, the versioning is related to the Event and not to the API.
/// </summary>
public class OrganizationCreatedEvent : DomainEventBaseV2<OrganizationCreatedPayload>
{
    /// <summary>
    ///     Initializes a new instance of <see cref="OrganizationCreatedEvent" />.
    /// </summary>
    public OrganizationCreatedEvent()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="OrganizationCreatedEvent" />.
    /// </summary>
    /// <param name="timestamp">Specifies the time when the Event was created.</param>
    /// <param name="payload">Specifies the data of the created organization as a <see cref="OrganizationCreatedEvent" />.</param>
    public OrganizationCreatedEvent(DateTime timestamp, OrganizationCreatedPayload payload) : base(
        timestamp,
        payload)
    {
    }
}
