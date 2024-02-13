using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

/// <summary>
///     The first level creator creates the <see cref="EventTuple" />s for the
///     first level projection out of the <see cref="IUserProfileServiceEvent" />s and given metadata.
/// </summary>
public interface IFirstLevelEventTupleCreator
{
    /// <summary>
    ///     Creates out of an <inheritdoc cref="IUserProfileServiceEvent" /> and metadata
    ///     an <see cref="EventTuple" />.
    /// </summary>
    /// <param name="resourceId">The resource id for creating a stream name for the event.</param>
    /// <param name="upsEvent">
    ///     The <see cref="IUserProfileServiceEvent" /> that will be wrapped in an
    ///     <see cref="IUserProfileServiceEvent" />s.
    /// </param>
    /// <param name="domainEvent">
    ///     Base domain event to extract new metadata from (optional). If none provided, default values
    ///     will be taken.
    /// </param>
    /// <returns>The wrapped event (<see cref="EventTuple" />).</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resourceId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="upsEvent" /> is <c>null</c>
    /// </exception>
    EventTuple CreateEvent(
        ObjectIdent resourceId,
        IUserProfileServiceEvent upsEvent,
        IDomainEvent domainEvent = null);

    /// <summary>
    ///     Create out of <inheritdoc cref="IUserProfileServiceEvent" />s and metadata <see cref="EventTuple" />s.
    /// </summary>
    /// <param name="resourceId">The resource id for creating a stream name for the event.</param>
    /// <param name="upsEvents">
    ///     The <see cref="IUserProfileServiceEvent" /> that will be wrapped in an
    ///     <see cref="IUserProfileServiceEvent" />s.
    /// </param>
    /// <param name="domainEvent">
    ///     Base domain event to extract new metadata from (optional). If none provided, default values
    ///     will be taken.
    /// </param>
    /// <returns>The wrapped event (<see cref="EventTuple" />s).</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resourceId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="upsEvents" /> is <c>null</c>
    /// </exception>
    IEnumerable<EventTuple> CreateEvents(
        ObjectIdent resourceId,
        IEnumerable<IUserProfileServiceEvent> upsEvents,
        IDomainEvent domainEvent = null);
}
