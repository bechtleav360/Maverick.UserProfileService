﻿using System;
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
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Services;

public class AssignmentsSecondLevelProjectionService : ProjectionBase, IAssignmentsSecondLevelProjection
{
    /// <inheritdoc />
    protected override bool UseAllStreams => true;

    /// <summary>
    ///     Initializes a new instance of <see cref="AssignmentsSecondLevelProjectionService" />.
    /// </summary>
    /// <param name="logger">Logger that will accept logging messages.</param>
    /// <param name="activitySourceWrapper">The <see cref="IActivitySourceWrapper" /></param>
    /// <param name="eventStorageConfiguration">The eventstore configuration.</param>
    /// <param name="serviceProvider">THe service provider which will be used in order to create a scope per event.</param>
    public AssignmentsSecondLevelProjectionService(
        ILogger<AssignmentsSecondLevelProjectionService> logger,
        IActivitySourceWrapper activitySourceWrapper,
        IOptionsMonitor<MartenEventStoreOptions> eventStorageConfiguration,
        IServiceProvider serviceProvider) : base(
        logger,
        activitySourceWrapper,
        serviceProvider,
        eventStorageConfiguration)
    {
    }

    /// <inheritdoc />
    protected override async Task<Dictionary<string, ulong>> GetLatestProjectedEventPositionAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISecondLevelAssignmentRepository>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        Dictionary<string, ulong> events = await repository.GetLatestProjectedEventIdsAsync(cancellationToken);

        return Logger.ExitMethod(events);
    }

    protected override async Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISecondLevelAssignmentRepository>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        GlobalPosition latestPosition =
            await repository.GetPositionOfLatestProjectedEventAsync(cancellationToken);

        return Logger.ExitMethod(latestPosition);
    }

    /// <inheritdoc />
    protected override async Task HandleDomainEventAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var secondLevelEventHandler = scope.ServiceProvider.GetRequiredService<ISecondLevelAssignmentEventHandler>();

        try
        {
            await secondLevelEventHandler.HandleEventAsync(domainEvent, eventHeader, cancellationToken);
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

    /// <inheritdoc />
    protected override void OnExceptionOccurred(HealthStatus newStatus, Exception exception)
    {
    }

    /// <inheritdoc />
    protected override void SetHealthStatus(HealthStatus status, string message = null)
    {
    }
}
