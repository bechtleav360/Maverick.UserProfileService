using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Common;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.EventSourcing.Abstractions.Stores;

/// <summary>
///     The even stored operation which are needed to write/read in/from
///     the event store.
/// </summary>
public interface IEventStorageClient
{
    /// <summary>
    ///     Write a DomainEvent into the event store.
    /// </summary>
    /// <param name="domainEvent">Event that should be written in the eventStore.</param>
    /// <param name="streamName">The name of the stream where the events has to be written.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task<WriteEventResult> WriteEventAsync(
        IUserProfileServiceEvent domainEvent,
        string streamName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Write a DomainEvent into the event store.
    /// </summary>
    /// <param name="domainEvent">Event that should be written in the eventStore.</param>
    /// <param name="streamName">The name of the stream where the events has to be written.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task<WriteEventResult> WriteEventAsync<TEventType>(
        TEventType domainEvent,
        string streamName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Write multiple DomainEvents into the event store.
    /// </summary>
    /// <param name="domainEvents">Event that should be written in the eventStore.</param>
    /// <param name="streamName">The name of the stream where the events has to be written.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task<WriteEventResult> WriteEventsAsync(
        IList<IUserProfileServiceEvent> domainEvents,
        string streamName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Load event corresponding to the given event id.
    /// </summary>
    /// <param name="eventId">   The id of the event that should be loaded</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A <see cref="LoadEventAsync" /> containing the event.</returns>
    Task<LoadEventResult> LoadEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the last event that occured on a stream.
    /// </summary>
    /// <typeparam name="T">The expected type of the event.</typeparam>
    /// <param name="streamName">The name of the stream from which the last event is retrieved.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     The last event that occurred on the specified stream or <c>null</c> if the stream does not exist.
    /// </returns>
    Task<T?> GetLastEventFromStreamAsync<T>(string streamName, CancellationToken cancellationToken = default)
        where T : IUserProfileServiceEvent;

    /// <summary>
    ///     Returns the name of the stream the event are written.
    /// </summary>
    /// <returns>The name of the stream the events are written.</returns>
    string GetDefaultStreamName();

    /// <summary>
    ///     Archives (deletes soft) the stream with the given name.
    /// </summary>
    /// <param name="streamName">   The name of the stream that should be archived</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <remarks>
    ///     After archiving the stream will not be deleted from the database,
    ///     but it will be marked with the flag and won't be accessible from the application anymore.
    /// </remarks>
    /// <returns> A <see cref="Task" /> will be returned.</returns>
    Task SoftDeleteStreamAsync(string streamName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tests if a stream exists.
    /// </summary>
    /// <param name="streamName">
    ///     The name of the stream that is tested.
    /// </param>
    /// <returns>
    ///     <c>True</c> if the stream named <paramref name="streamName" /> exists, otherwise <c>False</c>.
    /// </returns>
    Task<bool> StreamExistsAsync(string streamName);

    /// <summary>
    ///     Validates the events for the event store to make sure that they do not throw an error in the write process.
    /// </summary>
    /// <param name="events">Collection of events to validate.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>True if events are valid, otherwise false.</returns>
    // TODO: Add validation result object.
    Task<bool> ValidateEventsAsync(ICollection<EventTuple> events, CancellationToken cancellationToken = default);
}
