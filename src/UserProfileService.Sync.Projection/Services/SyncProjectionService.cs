using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Services;

/// <summary>
///     A default implementation of <see cref="ProjectionBase" /> for the UPS-Sync.
/// </summary>
public class SyncProjectionService : ProjectionBase, ISyncProjection
{
    private readonly IDbInitializer _dbInitializer;
    private readonly ProjectionServiceHealthCheck _healthStore;
    private readonly IProfileService _profileService;

    /// <summary>
    ///     All streams are needed to get the events.
    /// </summary>
    protected override bool UseAllStreams => true;

    /// <summary>
    ///     Creates a new instance of <see cref="SyncProjectionService" />
    /// </summary>
    /// <param name="logger">The logger <see cref="ILogger" />.</param>
    /// <param name="profileService"> A service used to handle user operations.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" /> used to retrieved required third services.</param>
    /// <param name="activitySourceWrapper">Wrapper to wrap the activity source to use dependency injection.</param>
    /// <param name="healthStore">Store to set health status.</param>
    /// <param name="dbInitializer">
    ///     Initializes a database before it is being used. This operation can contain checks, creation or modification of
    ///     stored schemes, table, collections, etc.
    /// </param>
    /// <param name="eventStorageConfiguration">Object that provides the configuration for the event store</param>
    public SyncProjectionService(
        ILogger<SyncProjectionService> logger,
        IProfileService profileService,
        IServiceProvider serviceProvider,
        IActivitySourceWrapper activitySourceWrapper,
        ProjectionServiceHealthCheck healthStore,
        IDbInitializer dbInitializer,
        IOptionsMonitor<MartenEventStoreOptions> eventStorageConfiguration) : base(
        logger,
        activitySourceWrapper,
        serviceProvider,
        eventStorageConfiguration)
    {
        _dbInitializer = dbInitializer;
        _profileService = profileService;
        _healthStore = healthStore;
    }

    /// <inheritdoc />
    protected override async Task<Dictionary<string, ulong>> GetLatestProjectedEventPositionAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        Dictionary<string, ulong> result = await _profileService.GetLatestProjectedEventIdsAsync(cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    protected override async Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        GlobalPosition events =
            await _profileService.GetPositionOfLatestProjectedEventAsync(cancellationToken);

        return Logger.ExitMethod(events);
    }

    /// <inheritdoc />
    protected override async Task HandleDomainEventAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var syncProjectionEventHandler = scope.ServiceProvider.GetRequiredService<ISyncProjectionEventHandler>();

        try
        {
            await syncProjectionEventHandler.HandleEventAsync(domainEvent, eventHeader, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "An error occurred while handling domain event with id {id}, type {type} and stream {stream}. Response with exception will be send.",
                LogHelpers.Arguments(eventHeader.EventId, eventHeader.EventType, eventHeader.StreamId));

            throw;
        }
        finally
        {
            Logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    protected override void OnExceptionOccurred(HealthStatus newStatus, Exception exception)
    {
        Logger.LogWarnMessage(
            exception,
            "The health has changed to {newStatus}",
            newStatus.ToLogString().AsArgumentList());
    }

    /// <inheritdoc />
    protected override void SetHealthStatus(HealthStatus newStatus, string message = null)
    {
        _healthStore.Status = newStatus;
        _healthStore.Message = message;
    }

    /// <inheritdoc />
    protected override bool TryGetStreamNamePattern(out Regex regEx)
    {
        using IServiceScope scope = ServiceProvider.CreateScope();
        var streamNameResolver = scope.ServiceProvider.GetService<IStreamNameResolver>();

        if (streamNameResolver == null)
        {
            regEx = default;

            return false;
        }

        regEx = streamNameResolver.GetStreamNamePattern();

        return true;
    }
}
