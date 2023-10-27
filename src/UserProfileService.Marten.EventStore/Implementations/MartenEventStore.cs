using System.Data;
using Marten;
using Marten.Events;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Marten.EventStore.Options;

namespace UserProfileService.Marten.EventStore.Implementations;

/// <summary>
///     An implementation of <see cref="IEventStorageClient" /> using the marten library.
/// </summary>
public class MartenEventStore : IEventStorageClient
{
    private readonly IDocumentStore _documentStore;
    private readonly MartenEventStoreOptions? _eventStorageOptions;
    private readonly ILogger<MartenEventStore> _logger;

    /// <summary>
    ///     Creates a new instance of <see cref="MartenEventStore" />.
    /// </summary>
    /// <param name="documentStore">
    ///     Provides access to Marten's event store.
    /// </param>
    /// <param name="provider">
    ///     Service provider used to get registered services from the IoC container.
    /// </param>
    /// <param name="logger">
    ///     The logger.
    /// </param>
    public MartenEventStore(
        IDocumentStore documentStore,
        IServiceProvider provider,
        ILogger<MartenEventStore> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
        IServiceScope scope = provider.CreateScope();

        _eventStorageOptions =
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<MartenEventStoreOptions>>().Value;
    }

    private async Task<WriteEventResult> WriteEventDataAsync<T>(
        T data,
        string streamName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(nameof(streamName)))
        {
            throw new ArgumentException("The stream name should not be empty or whitespace");
        }

        try
        {
            await using IDocumentSession session = _documentStore.LightweightSession();

            (bool Exists, bool IsArchived, long Version) streamState = await GetStreamStateAsync(
                streamName,
                session);

            StreamAction actionResult;

            if (!streamState.Exists)
            {
                _logger.LogInfoMessage(
                    "The stream {StreamName} doesn't exist and will be created/started",
                    LogHelpers.Arguments(streamName));

                actionResult = session.Events.StartStream(streamName, data);
            }

            else if (streamState.IsArchived)
            {
                var ex = new AccessDeletedStreamException(streamName);

                _logger.LogErrorMessage(
                    ex,
                    "Error: trying to write in the (deleted/archived) stream {StreamName} ",
                    LogHelpers.Arguments(streamName));

                throw ex;
            }
            else
            {
                actionResult = session.Events.Append(streamName, data);
            }

            if (actionResult == null)
            {
                var exception =
                    new AggregateException($"Write operation inside the stream: {streamName} returned null");

                _logger.LogErrorMessage(
                    exception,
                    "Error: writing operation inside the stream {StreamName} return null ",
                    LogHelpers.Arguments(streamName));

                throw exception;
            }

            await session.SaveChangesAsync(cancellationToken);

            return _logger.ExitMethod(
                new WriteEventResult
                {
                    CurrentVersion = actionResult.Version,
                    EventId = actionResult.Events.FirstOrDefault()?.Id,
                    Sequence = actionResult.Events.FirstOrDefault()?.Sequence
                });
        }
        //TODO: exception handling with marten specified and own exceptions
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by writing event inside the stream {streamName}",
                LogHelpers.Arguments(streamName));

            throw;
        }
    }

    private static async Task<(bool Exists, bool IsArchived, long Version)> GetStreamStateAsync(
        string streamName,
        IDocumentSession session)
    {
        StreamState? state = await session.Events.FetchStreamStateAsync(streamName);

        return (state != null, state?.IsArchived ?? false, state?.Version ?? 0);
    }

    /// <inheritdoc />
    public async Task<LoadEventResult> LoadEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (eventId.Equals(Guid.Empty))
        {
            throw new ArgumentException("The event id should not be empty");
        }

        try
        {
            _logger.LogInfoMessage(
                "Trying to load event from marten event store with the following Id: {eventId}",
                LogHelpers.Arguments(eventId.ToString()));

            await using IDocumentSession session = _documentStore.LightweightSession();

            IEvent? loadedEvent = await session.Events.LoadAsync(eventId, cancellationToken);

            if (loadedEvent == null)
            {
                throw new DataException($"Could not load event data of event '{eventId}'");
            }

            _logger.LogInfoMessage(
                "Successfully loaded event from marten event store with the following Id: {eventId}",
                LogHelpers.Arguments(eventId.ToString()));

            return new LoadEventResult
            {
                Id = loadedEvent.Id,
                Sequence = loadedEvent.Sequence,
                Data = loadedEvent.Data,
                StreamId = loadedEvent.StreamId,
                StreamKey = loadedEvent.StreamKey,
                Timestamp = loadedEvent.Timestamp,
                Version = loadedEvent.Version
            };
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by loading event with id:{eventId}",
                LogHelpers.Arguments(eventId.ToString()));

            throw;
        }
    }

    /// <inheritdoc />
    public async Task SoftDeleteStreamAsync(string streamName, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        await using IDocumentSession session = _documentStore.LightweightSession();
        (bool exists, bool archived, long _) = await GetStreamStateAsync(streamName, session);

        if (!exists)
        {
            throw new EventStreamNotFoundException(streamName);
        }

        if (archived)
        {
            throw new AccessDeletedStreamException(streamName);
        }

        try
        {
            _logger.LogInfoMessage(
                "Archiving the stream with the name: {streamName}",
                LogHelpers.Arguments(streamName));

            session.Events.ArchiveStream(streamName);

            await session.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by archiving stream with name: {streamName}",
                LogHelpers.Arguments(streamName));

            throw;
        }
        finally
        {
            _logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task<WriteEventResult> WriteEventAsync(
        IUserProfileServiceEvent domainEvent,
        string streamName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        try
        {
            _logger.LogInfoMessage(
                "Adding event with id: {eventId} and type: {eventType} inside the stream: {streamName}",
                LogHelpers.Arguments(domainEvent.EventId, domainEvent.Type, streamName));

            return _logger.ExitMethod(
                await WriteEventDataAsync(
                    domainEvent,
                    streamName,
                    cancellationToken));
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by writing event of type:{type}, with the id: {id} inside the stream {streamName}",
                LogHelpers.Arguments(domainEvent.Type, domainEvent.EventId, streamName));

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<WriteEventResult> WriteEventAsync<TEventType>(
        TEventType domainEvent,
        string streamName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        try
        {
            return _logger.ExitMethod(
                await WriteEventDataAsync(
                    domainEvent,
                    streamName,
                    cancellationToken));
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by writing event of type:{type}, inside the stream {streamName}",
                LogHelpers.Arguments(typeof(TEventType), streamName));

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<WriteEventResult> WriteEventsAsync(
        IList<IUserProfileServiceEvent> domainEvents,
        string streamName,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentNullException(nameof(streamName));
        }

        if (domainEvents == null)
        {
            throw new ArgumentNullException(nameof(domainEvents));
        }

        if (domainEvents.Count == 0)
        {
            throw new ArgumentException("The list containing events to add should not be empty");
        }

        try
        {
            return _logger.ExitMethod(
                await WriteEventDataAsync(
                    domainEvents,
                    streamName,
                    cancellationToken));
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "Error happened by writing events of type:{type}, inside the stream {streamName}",
                LogHelpers.Arguments(typeof(IUserProfileServiceEvent), streamName));

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetLastEventFromStreamAsync<T>(
        string streamName,
        CancellationToken cancellationToken = default)
        where T : IUserProfileServiceEvent
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(nameof(streamName)))
        {
            throw new ArgumentException("The stream name should not be empty or whitespace");
        }

        try
        {
            await using IDocumentSession session = _documentStore.LightweightSession();
            (bool exists, bool isArchived, long version) = await GetStreamStateAsync(streamName, session);

            if (!exists)
            {
                _logger.ExitMethod();

                throw new EventStreamNotFoundException(streamName);
            }

            if (isArchived)
            {
                _logger.ExitMethod();

                throw new AccessDeletedStreamException(streamName);
            }

            _logger.LogInfoMessage(
                "Getting the last event of the stream {streamName}",
                LogHelpers.Arguments(streamName));

            IReadOnlyList<IEvent> events = await session.Events.FetchStreamAsync(
                streamName,
                fromVersion: version,
                token: cancellationToken);

            return (T)events[0].Data;
        }
        catch (AccessDeletedStreamException accessEx)
        {
            _logger.LogErrorMessage(
                accessEx,
                " Error happened by getting the last event of the stream {streamName}, stream is archived",
                LogHelpers.Arguments(streamName));

            _logger.ExitMethod();

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogErrorMessage(
                ex,
                "Error trying to fetch last event from stream {streamName}.",
                LogHelpers.Arguments(streamName));

            _logger.ExitMethod();

            throw;
        }
    }

    /// <inheritdoc />
    public string GetDefaultStreamName()
    {
        _logger.EnterMethod();

        _logger.LogInfoMessage(
            "Getting the default stream name from the event storage configuration",
            LogHelpers.Arguments());

        return _eventStorageOptions?.SubscriptionName ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<bool> StreamExistsAsync(string streamName)
    {
        _logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("The stream name should not be null or white space");
        }

        await using IDocumentSession session = _documentStore.LightweightSession();
        (bool exists, _, _) = await GetStreamStateAsync(streamName, session);

        return _logger.ExitMethod(exists);
    }

    /// <inheritdoc />
    public Task<bool> ValidateEventsAsync(
        ICollection<EventTuple> events,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        bool result = events.All(
            eventTuple => eventTuple.Event != null && !string.IsNullOrWhiteSpace(eventTuple.TargetStream));

        return _logger.ExitMethod(Task.FromResult(result));
    }
}
