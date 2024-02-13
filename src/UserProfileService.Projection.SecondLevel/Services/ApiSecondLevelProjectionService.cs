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
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Services;

/// <summary>
///     Service handling <see cref="IUserProfileServiceEvent"/>s for the second level projection of the API. 
/// </summary>
public class ApiSecondLevelProjectionService : ProjectionBase, ISecondLevelProjection
{
    /// <inheritdoc />
    protected override bool UseAllStreams => true;

    /// <summary>
    ///     Initializes a new instance of <see cref="ApiSecondLevelProjectionService" />.
    /// </summary>
    /// <param name="logger">Logger that will accept logging messages.</param>
    /// <param name="activitySourceWrapper"></param>
    /// <param name="eventStorageConfiguration"></param>
    /// <param name="serviceProvider"></param>
    public ApiSecondLevelProjectionService(
        ILogger<ApiSecondLevelProjectionService> logger,
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
        var repository = scope.ServiceProvider.GetRequiredService<ISecondLevelProjectionRepository>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await initializer.EnsureDatabaseAsync(cancellationToken: cancellationToken);

        Dictionary<string, ulong> events = await repository.GetLatestProjectedEventIdsAsync(cancellationToken);

        return Logger.ExitMethod(events);
    }

    /// <inheritdoc />
    protected override async Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISecondLevelProjectionRepository>();
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
        var secondLevelEventHandler = scope.ServiceProvider.GetRequiredService<ISecondLevelEventHandler>();

        try
        {
            await secondLevelEventHandler.HandleEventAsync(domainEvent, eventHeader, cancellationToken);

            await SendResponseAsync(
                (responseService, e, ct) =>
                    responseService.ResponseAsync(e, ct),
                domainEvent,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInfoMessage(
                "Operation has been cancelled (during event stream: {eventStream}; event number: {eventNumber}; event name: {eventName}).",
                LogHelpers.Arguments(
                    eventHeader?.StreamId,
                    eventHeader?.EventNumberVersion,
                    eventHeader?.EventType));
        }
        catch (Exception exc)
        {
            Logger.LogErrorMessage(
                exc,
                "An error occurred while handling domain event with id {id}, type {type} and stream {stream}. Response with exception will be send.",
                LogHelpers.Arguments(eventHeader.EventId, eventHeader.EventType, eventHeader.StreamId));

            await SendResponseAsync(
                (responseService, e, ct) =>
                    responseService.ResponseAsync(domainEvent, exc, cancellationToken),
                domainEvent,
                cancellationToken);

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
    }

    /// <inheritdoc />
    protected override void SetHealthStatus(HealthStatus status, string message = null)
    {
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

        regEx = streamNameResolver?.GetStreamNamePattern();

        return true;
    }
}
