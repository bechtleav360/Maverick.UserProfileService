using System.Text.RegularExpressions;
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
using UserProfileService.Projection.SecondLevel.VolatileDataStore.Abstractions;

namespace UserProfileService.Projection.SecondLevel.VolatileDataStore.Services;

/// <summary>
///     Represents the second-level projection service of the volatile data store.
/// </summary>
public class VolatileDataSecondLevelProjectionService : ProjectionBase, IVolatileDataSecondLevelProjection
{
    /// <inheritdoc />
    protected override bool UseAllStreams => true;

    /// <summary>
    ///     Initializes a new instance of <see cref="VolatileDataSecondLevelProjectionService" />.
    /// </summary>
    /// <param name="logger">Logger that will accept logging messages.</param>
    /// <param name="activitySourceWrapper">The <see cref="IActivitySourceWrapper" /></param>
    /// <param name="eventStorageConfiguration">The event store configuration.</param>
    /// <param name="serviceProvider">THe service provider which will be used in order to create a scope per event.</param>
    public VolatileDataSecondLevelProjectionService(
        ILogger<VolatileDataSecondLevelProjectionService> logger,
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
        var repository = scope.ServiceProvider.GetRequiredService<ISecondLevelVolatileDataRepository>();

        Dictionary<string, ulong> events = await repository.GetLatestProjectedEventIdsAsync(cancellationToken);

        return Logger.ExitMethod(events);
    }

    /// <inheritdoc />
    protected override Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This method is not supported by this implementation.");
    }

    /// <inheritdoc />
    protected override async Task HandleDomainEventAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        using IServiceScope scope = ServiceProvider.CreateScope();
        var secondLevelEventHandler = scope.ServiceProvider.GetRequiredService<ISecondLevelVolatileDataEventHandler>();

        try
        {
            await secondLevelEventHandler.HandleEventAsync(domainEvent, eventHeader, cancellationToken);
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

            throw;
        }
        finally
        {
            Logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    protected override bool TryGetStreamNamePattern(out Regex? regEx)
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
    protected override void SetHealthStatus(HealthStatus status, string? message = null)
    {
    }
}
