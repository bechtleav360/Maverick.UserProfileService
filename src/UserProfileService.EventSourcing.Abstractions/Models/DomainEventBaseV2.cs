using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Attributes;

namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <inheritdoc cref="DomainEvent{TPayload}" />
[EventVersion(2)]
public abstract class DomainEventBaseV2<TPayload> : DomainEvent<TPayload>, IUserProfileServiceEvent
{
    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV2{TPayload}" /> and sets the
    ///     <see cref="DomainEvent{TPayload}.VersionInformation" />.
    /// </summary>
    public DomainEventBaseV2()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV2{TPayload}" /> and sets the
    ///     <see cref="DomainEvent{TPayload}.VersionInformation" />.
    /// </summary>
    /// <param name="timestamp">The timestamp as a <see cref="DateTime" />, when the event was raised.</param>
    /// <param name="payload">The payload carried by the event as a {TPayload}.</param>
    public DomainEventBaseV2(DateTime timestamp, TPayload payload) : base(timestamp, payload)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="DomainEventBaseV2{TPayload}" /> and sets the
    ///     <see cref="DomainEvent{TPayload}.VersionInformation" />.
    /// </summary>
    /// <param name="timestamp">The timestamp as a <see cref="DateTime" />, when the event was raised.</param>
    /// <param name="payload">The payload carried by the event as a {TPayload}.</param>
    /// <param name="correlationId">The correlation id assigned to all related events.</param>
    public DomainEventBaseV2(DateTime timestamp, TPayload payload, string correlationId) : base(
        timestamp,
        payload,
        correlationId)
    {
    }
}
