using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.EventSourcing.Abstractions.Stores;

namespace UserProfileService.EventSourcing.Abstractions.BasicImplementations;

internal class EventStoreEventPublisher : IEventPublisher
{
    private readonly IEventStorageClient _Client;
    private readonly ILogger<EventStoreEventPublisher> _Logger;

    /// <inheritdoc />
    public bool IsDefault => true;

    /// <inheritdoc />
    public string Type => "default";

    /// <summary>
    ///     Initializes a new instance of <see cref="EventStoreEventPublisher" />.
    /// </summary>
    /// <param name="client">The event store client to be used.</param>
    /// <param name="logger">The logger to be used.</param>
    public EventStoreEventPublisher(
        IEventStorageClient client,
        ILogger<EventStoreEventPublisher> logger)
    {
        _Logger = logger;
        _Client = client;
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        IUserProfileServiceEvent eventData,
        EventPublisherContext context,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (eventData == null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }

        if (string.IsNullOrWhiteSpace(eventData.EventId))
        {
            throw new ArgumentException(
                $"Parameter {nameof(eventData)} misses a valid event id",
                nameof(eventData));
        }

        if (string.IsNullOrWhiteSpace(eventData.Type))
        {
            throw new ArgumentException(
                $"Parameter {nameof(eventData)} misses a valid event type name",
                nameof(eventData));
        }

        await _Client.WriteEventAsync(
            eventData,
            _Client.GetDefaultStreamName(),
            cancellationToken);

        _Logger.LogInfoMessage(
            "Event data [id = {eventId}; type = {eventType}] written successfully - correlation id: {correlationId}",
            LogHelpers.Arguments(
                eventData.EventId,
                eventData.Type,
                eventData.MetaData?.CorrelationId));

        _Logger.ExitMethod();
    }
}
