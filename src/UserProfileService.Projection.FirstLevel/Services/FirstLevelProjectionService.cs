using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Projection.FirstLevel.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Services;

/// <summary>
///     First level event projection projects all events
///     in one main stream. From that point the event splitting
///     in various streams take place. Every domain event in
///     getting one separate stream.
/// </summary>
public class FirstLevelProjectionService : ProjectionBase, IFirstLevelProjection
{
    private readonly IDbInitializer _DbInitializer;
    private readonly IFirstLevelProjectionRepository _FirstLevelProjectionRepository;
    private readonly ProjectionServiceHealthCheck _HealthStore;
    private readonly IFirstLevelProjectionEventHandler _MainFirstLevelProjectionHandler;

    /// <summary>
    ///     Only one stream is needed to get the events.
    /// </summary>
    protected override bool UseAllStreams => false;

    public FirstLevelProjectionService(
        ILogger<FirstLevelProjectionService> logger,
        IActivitySourceWrapper activitySourceWrapper,
        IOptionsMonitor<MartenEventStoreOptions> eventStorageConfiguration,
        IFirstLevelProjectionRepository firstLevelProjectionRepository,
        IDbInitializer dbInitializer,
        ProjectionServiceHealthCheck healthStore,
        IFirstLevelProjectionEventHandler mainFirstLevelProjectionHandler,
        IServiceProvider serviceProvider) : base(
        logger,
        activitySourceWrapper,
        serviceProvider,
        eventStorageConfiguration)
    {
        _FirstLevelProjectionRepository = firstLevelProjectionRepository;
        _DbInitializer = dbInitializer;
        _HealthStore = healthStore;
        _MainFirstLevelProjectionHandler = mainFirstLevelProjectionHandler;
    }

    protected override async Task<Dictionary<string, ulong>> GetLatestProjectedEventPositionAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _DbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        Dictionary<string, ulong> events =
            await _FirstLevelProjectionRepository.GetLatestProjectedEventIdsAsync(cancellationToken);

        return Logger.ExitMethod(events);
    }

    /// <inheritdoc />
    protected override async Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _DbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        GlobalPosition latestPosition =
            await _FirstLevelProjectionRepository.GetPositionOfLatestProjectedEventAsync(cancellationToken);

        return Logger.ExitMethod(latestPosition);
    }

    protected override async Task HandleDomainEventAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        try
        {
            await _DbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

            await _MainFirstLevelProjectionHandler.HandleEventAsync(domainEvent, eventHeader, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInfoMessage(
                "Operation has been cancelled (during event number: {eventNumber}; event name: {eventName}).",
                LogHelpers.Arguments(eventHeader?.EventNumberVersion, eventHeader?.EventType));
        }
        catch (Exception exc)
        {
            await SendResponseAsync(
                (responseService, e, ct) => responseService.ResponseAsync(e, exc, ct),
                domainEvent,
                cancellationToken);

            throw;
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    protected override void OnExceptionOccurred(HealthStatus newStatus, Exception exception)
    {
    }

    /// <inheritdoc />
    protected override void SetHealthStatus(HealthStatus status, string message = null)
    {
        _HealthStore.Status = status;
        _HealthStore.Message = message;
    }
}
