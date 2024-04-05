using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Marten;
using Marten.Events;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Health;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Saga.Worker.Configuration;
using EventInitiator = UserProfileService.EventSourcing.Abstractions.Models.EventInitiator;
using InitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;

namespace UserProfileService.Saga.Worker.Setup;

/// <summary>
///     Background service to write data initially to the event store.
/// </summary>
public class SeedingService : BackgroundService
{
    private readonly IDocumentStore _documentStorage;
    private readonly IEventStorageClient _eventStorageClient;
    private readonly MartenEventStoreOptions _eventStoreConfiguration;
    private readonly IHealthStore _healthStatusStore;
    private readonly ILogger<SeedingService> _logger;
    private readonly IMapper _mapper;
    private readonly IDictionary<Type, ISet<string>> _seededEntities = new Dictionary<Type, ISet<string>>();
    private readonly SeedingConfiguration _seedingConfiguration;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Create an instance of <see cref="SeedingService" />
    /// </summary>
    /// <param name="serviceProvider">Provider for retrieving service objects.</param>
    /// <param name="seedingOptions">Options to configure the seeding process.</param>
    /// <param name="eventStoreOptions">Options to configure the marten event store.</param>
    /// <param name="eventStorageClient"></param>
    /// <param name="healthStatusStore"></param>
    /// <param name="documentStorage"></param>
    public SeedingService(
        IServiceProvider serviceProvider,
        IOptions<SeedingConfiguration> seedingOptions,
        IOptions<MartenEventStoreOptions> eventStoreOptions,
        IEventStorageClient eventStorageClient,
        IHealthStore healthStatusStore,
        IDocumentStore documentStorage)
    {
        _seedingConfiguration = seedingOptions.Value;
        _eventStoreConfiguration = eventStoreOptions.Value;
        _serviceProvider = serviceProvider;
        _healthStatusStore = healthStatusStore;
        _eventStorageClient = eventStorageClient;
        _documentStorage = documentStorage;
        _logger = serviceProvider.GetRequiredService<ILogger<SeedingService>>();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    private void CheckEntity<TEntity>(IReadOnlyDictionary<string, TEntity> entities, string entityId)
    {
        if (!entities.ContainsKey(entityId))
        {
            return;
        }

        if (!_seededEntities.TryGetValue(typeof(TEntity), out ISet<string> seededEntity))
        {
            seededEntity = new HashSet<string>();
        }

        seededEntity.Add(entityId);
        _seededEntities[typeof(TEntity)] = seededEntity;
    }

    private async Task HandleDomainEvent(
        StreamedEventHeader header,
        IUserProfileServiceEvent domainEvent,
        int lastEventVersion)
    {
        _logger.EnterMethod();

        _logger.LogTraceMessage(
            "Handling the event: {header.EventType}. Event number: {header.EventNumberVersion}. Event id: {header.EventNumberSequence}.",
            LogHelpers.Arguments(header.EventType, header.EventNumberVersion, header.EventNumberSequence));

        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            switch (domainEvent)
            {
                case TagCreatedEvent tagCreatedEvent:
                    CheckEntity(_seedingConfiguration.Tags, tagCreatedEvent.Payload.Id);

                    break;
                case FunctionCreatedEvent functionCreatedEvent:
                    CheckEntity(_seedingConfiguration.Functions, functionCreatedEvent.Payload.Id);

                    break;
                case GroupCreatedEvent groupCreatedEvent:
                    CheckEntity(_seedingConfiguration.Groups, groupCreatedEvent.Payload.Id);

                    break;
                case UserCreatedEvent userCreatedEvent:
                    CheckEntity(_seedingConfiguration.Users, userCreatedEvent.Payload.Id);

                    break;
                case RoleCreatedEvent roleCreatedEvent:
                    CheckEntity(_seedingConfiguration.Roles, roleCreatedEvent.Payload.Id);

                    break;
            }

            if (lastEventVersion == header.EventNumberVersion)
            {
                await HandleStreamHeadReached(header);
            }
        }
        catch (Exception ex)
        {
            _logger.LogErrorMessage(
                ex,
                "An error has occurred while processing the event with id '{header.EventId}' and type '{header.EventType}'.",
                LogHelpers.Arguments(header.EventId, header.EventType));
        }

        _logger.ExitMethod();
    }

    private async Task HandleStreamHeadReached(StreamedEventHeader eventHeader)
    {
        _logger.LogInfoMessage(
            eventHeader == null
                ? "Stream head reached for subscription '{_eventStoreConfiguration.SubscriptionName}' without events."
                : "Stream head reached for subscription '{_eventStoreConfiguration.SubscriptionName}' and event '{eventHeader.EventId}'.",
            eventHeader == null
                ? LogHelpers.Arguments(_eventStoreConfiguration.SubscriptionName)
                : LogHelpers.Arguments(_eventStoreConfiguration.SubscriptionName, eventHeader.EventId));

        var correlationId = Guid.NewGuid().ToString();

        _logger.LogInfoMessage(
            "Start seeding with correlation identifier {correlationId}",
            LogHelpers.Arguments(correlationId));

        await ProcessEntity<CreateTagRequest, TagCreatedEvent, TagCreatedPayload>(
            _seedingConfiguration.Tags,
            correlationId);

        await ProcessEntity<CreateOrganizationRequest, OrganizationCreatedEvent, OrganizationCreatedPayload>(
            _seedingConfiguration.Organizations,
            correlationId);

        await ProcessEntity<CreateGroupRequest, GroupCreatedEvent, GroupCreatedPayload>(
            _seedingConfiguration.Groups,
            correlationId);

        await ProcessEntity<CreateRoleRequest, RoleCreatedEvent, RoleCreatedPayload>(
            _seedingConfiguration.Roles,
            correlationId);

        await ProcessEntity<CreateFunctionRequest, FunctionCreatedEvent, FunctionCreatedPayload>(
            _seedingConfiguration.Functions,
            correlationId);

        await ProcessEntity<CreateUserRequest, UserCreatedEvent, UserCreatedPayload>(
            _seedingConfiguration.Users,
            correlationId);

        _healthStatusStore.SetHealthStatus(
            "seeding",
            new HealthState(HealthStatus.Healthy, DateTime.UtcNow, "The seeding finished successfully"));

        _logger.LogInfoMessage("Seeding completed successfully.", LogHelpers.Arguments());
        await StopAsync(default);
    }

    private async Task ProcessEntity<TEntity, TEvent, TPayload>(
        Dictionary<string, TEntity> entities,
        string correlationId)
        where TEvent : DomainEventBaseV2<TPayload>, new()
        where TPayload : ICreateModelPayload
    {
        _logger.LogInfoMessage(
            "Seeding entities for type '{type}'",
            LogHelpers.Arguments(typeof(TEntity).FullName));

        foreach (KeyValuePair<string, TEntity> entity in entities)
        {
            if (_seededEntities.TryGetValue(typeof(TEntity), out ISet<string> seededEntityIds)
                && seededEntityIds.Contains(entity.Key))
            {
                _logger.LogDebugMessage("Entity with id {id} is already seeded", LogHelpers.Arguments(entity.Key));

                continue;
            }

            _logger.LogInfoMessage(
                "Seeding entity with id '{id}' and type '{type}'",
                LogHelpers.Arguments(entity.Key, typeof(TEntity).FullName));

            try
            {
                var payload = _mapper.Map<TPayload>(entity.Value);
                payload.Id = entity.Key;

                await WriteGenericEvent<TEvent, TPayload>(payload, correlationId);
            }
            catch (Exception e)
            {
                _logger.LogErrorMessage(
                    e,
                    "An error occurred while seeding entity with type '{type}' and id '{id}'",
                    LogHelpers.Arguments(typeof(TEntity).FullName, entity.Key));

                throw;
            }
        }
    }

    private async Task WriteGenericEvent<TEvent, TPayload>(TPayload payload, string correlationId)
        where TEvent : DomainEventBaseV2<TPayload>, new()
    {
        var eventData = new TEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Initiator = new EventInitiator
            {
                Id = _seedingConfiguration.Id,
                Type = InitiatorType.System
            },
            Payload = payload,
            MetaData =
            {
                CorrelationId = correlationId,
                ProcessId = correlationId
            }
        };

        await _eventStorageClient.WriteEventAsync(eventData, _eventStorageClient.GetDefaultStreamName());

        _logger.LogDebugMessage(
            "Event with id '{eventId}' and type '{eventType}' was successfully sent to the store.",
            LogHelpers
                .Arguments(eventData.EventId, typeof(TEvent).Name));
    }

    /// <summary>
    ///     The methods executes the seeding process and validate if seeding is necessary.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInfoMessage("Start seeding for entities.", LogHelpers.Arguments());

        if (_seedingConfiguration.Disabled)
        {
            _logger.LogInfoMessage("The seeding was stopped because it is disabled.", LogHelpers.Arguments());
            await StopAsync(cancellationToken);

            return;
        }

        IDocumentSession documentSession = _documentStorage.LightweightSession();

        try
        {
            _logger.LogInfoMessage(
                "Getting the last event of the stream {streamName}.",
                LogHelpers.Arguments(_eventStoreConfiguration.SubscriptionName));

            int lastEventVersion = documentSession.Events
                .QueryAllRawEvents()
                .Count(e => e.StreamKey == _eventStoreConfiguration.SubscriptionName);

            _logger.LogInformation("The last event has the number {number}", lastEventVersion);

            if (lastEventVersion == 0)
            {
                _logger.LogDebugMessage(
                    "Stream '{stream}' is empty.",
                    LogHelpers.Arguments(_eventStoreConfiguration.SubscriptionName));

                await HandleStreamHeadReached(null);

                return;
            }

            _logger.LogDebugMessage(
                "Start building subscription for seeding data with stream '{stream}'",
                LogHelpers.Arguments(_eventStoreConfiguration.SubscriptionName));

            var from = 0;
            const int batchSize = 500;

            // We batch the event with a count of 500. The first level stream can
            // contain over 30k event.
            do
            {
                var batchEnd = from + batchSize;
                
                List<IEvent> eventsToHandle = documentSession.Events.QueryAllRawEvents()
                    .Where(
                        e => e.StreamKey
                            == _eventStoreConfiguration.SubscriptionName
                            && e.Version > from
                            && e.Version <= batchEnd)
                    .OrderBy(r => r.Version)
                    .ToList();

                foreach (IEvent @event in eventsToHandle)
                {
                    if (@event.Data is IUserProfileServiceEvent firstLevelEvent)
                    {
                        await HandleDomainEvent(
                            @event.ExtractStreamedEventHeader(),
                            firstLevelEvent,
                            lastEventVersion);
                    }
                }

                from += batchSize;
            }
            while (lastEventVersion > from);

            _logger.LogDebugMessage(
                "Building the subscription was successful, so events are analyzed and seeding starts.",
                LogHelpers.Arguments());
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(e, "Something went wrong when seeding the data.", LogHelpers.Arguments());

            _healthStatusStore.SetHealthStatus(
                "seeding",
                new HealthState(
                    HealthStatus.Healthy,
                    DateTime.UtcNow,
                    "An error occurred during the seeding process",
                    e));

            throw;
        }
    }
}
