using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Attributes;

namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <inheritdoc cref="DomainEvent{TPayload}" />
[EventVersion(3)]
public abstract class DomainEventBaseV3<TPayload> : DomainEvent<TPayload>, IUserProfileServiceEvent
{
    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV3{TPayload}" /> and sets the
    ///     <see cref="IUserProfileServiceEvent.VersionInformation" />.
    /// </summary>
    public DomainEventBaseV3()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV3{TPayload}" /> and sets the
    ///     <see cref="IUserProfileServiceEvent.VersionInformation" />.
    /// </summary>
    /// <param name="timestamp">The timestamp as a <see cref="DateTime" />, when the event was raised.</param>
    /// <param name="payload">The payload carried by the event as a {TPayload}.</param>
    public DomainEventBaseV3(DateTime timestamp, TPayload payload) : base(timestamp, payload)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV3{TPayload}" /> and sets the
    ///     <see cref="IUserProfileServiceEvent.VersionInformation" />.
    /// </summary>
    /// <param name="timestamp">The timestamp as a <see cref="DateTime" />, when the event was raised.</param>
    /// <param name="payload">The payload carried by the event as a {TPayload}.</param>
    /// <param name="correlationId">The correlation id assigned to all related events.</param>
    public DomainEventBaseV3(DateTime timestamp, TPayload payload, string correlationId) : base(
        timestamp,
        payload,
        correlationId)
    {
    }
}
