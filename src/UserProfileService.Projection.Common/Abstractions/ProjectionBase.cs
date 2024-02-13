using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;

namespace UserProfileService.Projection.Common.Abstractions;

/// <summary>
///     The base class for all projection background services that will handle <see cref="IUserProfileServiceEvent" />s.
/// </summary>
public abstract class ProjectionBase : IProjection
{
    private const int MaxNumberOfRequests = 1;
    private readonly IDisposable _changeToken;
    private readonly IDictionary<string, ulong> _initialLatestEvents = new Dictionary<string, ulong>();
    private readonly SemaphoreSlim _syncObject = new SemaphoreSlim(1, MaxNumberOfRequests);

    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    ///     The activity source wrapper to be used for this instance.
    /// </summary>
    protected IActivitySourceWrapper ActivitySourceWrapper { get; }

    /// <summary>
    ///     Gets the event storage client for this instance.
    /// </summary>
    protected IEventStorageClient EventStorage { get; }

    /// <summary>
    ///     Gets the current logger instance.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     Gets the configuration of event storage.
    /// </summary>
    protected MartenEventStoreOptions MartenEventStoreOptions { get; private set; }

    /// <summary>
    ///     Gets the stream name to be used for this projection. Default behavior: Using stream name from
    ///     <see cref="MartenEventStoreOptions" />.
    /// </summary>
    protected virtual string StreamName => MartenEventStoreOptions?.SubscriptionName;

    /// <summary>
    ///     Gets a boolean flag indicating whether all streams should be subscribed or not. If this is false,
    ///     <see cref="StreamName" /> must be set.
    /// </summary>
    protected abstract bool UseAllStreams { get; }

    /// <summary>
    ///     The DI service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     Is the default constructor of the base class <see cref="ProjectionBase" />.
    /// </summary>
    /// <param name="logger">The logger to be used.</param>
    /// <param name="activitySourceWrapper">The wrapper that contains the activity source used in this instance.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="martenEventStoreOptions">
    ///     The <see cref="IOptionsMonitor{TOptions}" /> that wraps the event storage
    ///     configuration.
    /// </param>
    protected ProjectionBase(
        ILogger logger,
        IActivitySourceWrapper activitySourceWrapper,
        IServiceProvider serviceProvider,
        IOptionsMonitor<MartenEventStoreOptions> martenEventStoreOptions)
    {
        Logger = logger;
        ActivitySourceWrapper = activitySourceWrapper;
        ServiceProvider = serviceProvider;
        _cancellationTokenSource = new CancellationTokenSource();
        _changeToken = martenEventStoreOptions.OnChange(OnConfigurationChanged);
        // Initial set config
        MartenEventStoreOptions = martenEventStoreOptions.CurrentValue;
    }

    private async void OnConfigurationChanged(MartenEventStoreOptions newConfiguration)
    {
        if (newConfiguration == null
            || newConfiguration.Equals(MartenEventStoreOptions))
        {
            return;
        }

        // Cancel waiting for old configuration change.
        CancellationTokenSource oldSource = _cancellationTokenSource;
        _cancellationTokenSource = null;
        oldSource?.Cancel();
        oldSource?.Dispose();

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _syncObject.WaitAsync(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Operation was cancelled by another configuration change.
            // Discard this change.
            return;
        }

        try
        {
            // to be on the safe side
            if (newConfiguration.Equals(MartenEventStoreOptions))
            {
                return;
            }

            MartenEventStoreOptions = newConfiguration;
        }
        finally
        {
            _syncObject.Release();
        }
    }

    /// <summary>
    ///     Handles all second level events
    /// </summary>
    /// <param name="streams">  Collection containing streams and corresponding events</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A <see cref="Task" /></returns>
    private async Task HandleMultipleStreamAsync(
        IReadOnlyCollection<StreamAction> streams,
        CancellationToken cancellationToken)
    {
        Dictionary<string, ulong> lastEventNumbers = await GetLatestProjectedEventPositionAsync(cancellationToken);

        List<IEvent> toHandleEvents = streams
            .Where(
                st => st.Key != null
                    && st.Key.StartsWith(MartenEventStoreOptions.StreamNamePrefix))
            .SelectMany(s => s.Events)
            .Where(e => e.FilterEvents(lastEventNumbers))
            .OrderBy(e => e.Sequence)
            .ToList();

        foreach (IEvent handleEvent in toHandleEvents)
        {
            await HandleEventInternal(handleEvent);
        }
    }

    /// <summary>
    ///     Handles all first level events
    /// </summary>
    /// <param name="streams">  Collection containing streams and corresponding events</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A <see cref="Task" /></returns>
    private async Task HandleSingleStreamAsync(
        IReadOnlyCollection<StreamAction> streams,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (streams == null)
        {
            throw new ArgumentException(nameof(streams));
        }

        if (streams.Count == 0)
        {
            Logger.LogWarning("The event list is empty, nothing to do.");

            return;
        }

        GlobalPosition lastEventNumber = await GetGlobalPositionOfLatestProjectedEventAsync(cancellationToken);
        StreamAction mainStream = streams.FirstOrDefault(st => st.Key == StreamName);

        if (mainStream != null && mainStream.Events.Count > 0)
        {
            foreach (IEvent mainStreamEvent in mainStream.Events)
            {
                if (mainStreamEvent.Version > lastEventNumber.Version)
                {
                    await HandleEventInternal(mainStreamEvent);
                }
            }
        }

        Logger.ExitMethod();
    }

    private async Task HandleEventInternal(IEvent @event)
    {
        var originalEvent = @event.Data as IUserProfileServiceEvent;
        StreamedEventHeader streamObject = @event.ExtractStreamedEventHeader();
        await OnEventReceivedAsync(streamObject, originalEvent);
    }

    /// <summary>
    ///     Performs a closing and cleanup for all resources in a safe way.
    /// </summary>
    /// <param name="disposing">Is true, if dispose has already been triggered.</param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _syncObject?.Dispose();
            _changeToken?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    ///     Returns the number of the latest projected event.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<Dictionary<string, ulong>> GetLatestProjectedEventPositionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the information about the global position of the latest projected event.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the information about the global position.</returns>
    protected abstract Task<GlobalPosition> GetGlobalPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Handles a domain event asynchronously.
    /// </summary>
    /// <param name="eventHeader">The streamed event header.</param>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    protected abstract Task HandleDomainEventAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Will be executed if a subscribed event storage stream receives a message.
    /// </summary>
    /// <param name="eventHeader">The event header that will be received from event storage.</param>
    /// <param name="domainEvent">The event that has been occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task OnEventReceivedAsync(
        StreamedEventHeader eventHeader,
        IUserProfileServiceEvent domainEvent)
    {
        Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (domainEvent.MetaData == null)
        {
            throw new ArgumentException(
                $"Domain event {domainEvent.GetType().FullName} has no meta data.",
                nameof(domainEvent));
        }

        if (UseAllStreams
            && _initialLatestEvents.TryGetValue(eventHeader.EventStreamId, out ulong eventNumber)
            && eventHeader.EventNumberVersion <= (long)eventNumber)
        {
            Logger.LogTraceMessage(
                "Skipping the event: {eventHeaderEventType}. Event number: {eventHeaderEventNumber}. Stream name: {streamName}. Event id: {eventHeaderEventId}.",
                LogHelpers.Arguments(
                    eventHeader.EventType,
                    eventHeader.EventNumberVersion,
                    eventHeader.StreamId,
                    eventHeader.EventId));

            return;
        }

        Activity activity = ActivitySourceWrapper.ActivitySource.StartActivity(
            $"{GetType().Name}: Handle event",
            ActivityKind.Producer,
            domainEvent.MetaData.CorrelationId);

        Logger.LogTraceMessage(
            "Handling the event: {eventHeaderEventType}. Event number: {eventHeaderEventNumber}. Stream name: {streamName}. Event id: {eventHeaderEventId}.",
            LogHelpers.Arguments(
                eventHeader.EventType,
                eventHeader.EventNumberVersion,
                eventHeader.StreamId,
                eventHeader.EventId));

        try
        {
            await HandleDomainEventAsync(eventHeader, domainEvent);
        }
        catch (ProjectionMissMatchException ex)
        {
            Logger.LogErrorMessage(
                ex,
                "An projection miss match error has occurred while processing the event {eventType} in {projectionType}. A new subscription will be build and started.",
                LogHelpers.Arguments(eventHeader.EventType, GetType().Name));

            SetHealthStatus(HealthStatus.Healthy);
        }
        // this kind of exception indicates there are issues in data sets or wrong queries
        // these problems are not related to network issues or problems with the database cluster/server
        // therefore the health status should not been changed
        catch (InstanceNotFoundException instanceNotFound)
        {
            Logger.LogDebugMessage(instanceNotFound,
                "InstanceNotFoundException occurred: {errorMessage} [code: {errorCode}, related id: {relatedId}]",
                LogHelpers.Arguments(
                    instanceNotFound.Message,
                    instanceNotFound.Code,
                    instanceNotFound.RelatedId));
        }
        catch (Exception ex)
        {
            Logger.LogErrorMessage(
                ex,
                "An error has occurred while processing the event {eventType} in {projectionType}.",
                LogHelpers.Arguments(
                    eventHeader.EventType,
                    GetType().Name));

            OnExceptionOccurred(HealthStatus.Degraded, ex);
        }

        activity?.Stop();

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Will be called, if an <paramref name="exception" /> occurs and the <see cref="HealthStatus" /> will be changed to
    ///     <paramref name="newStatus" />.
    /// </summary>
    /// <param name="newStatus">The new value of <see cref="HealthStatus" />.</param>
    /// <param name="exception">The exception that has been thrown.</param>
    protected abstract void OnExceptionOccurred(
        HealthStatus newStatus,
        Exception exception);

    /// <summary>
    ///     Will be called, if the health status has been changed. A optional <paramref name="message" /> can be set.
    /// </summary>
    /// <param name="newStatus">The new value of <see cref="HealthStatus" />.</param>
    /// <param name="message">An optional descriptive message why the health status has been changed.</param>
    protected abstract void SetHealthStatus(
        HealthStatus newStatus,
        string message = null);

    /// <summary>
    ///     Tries to get a regular expression to match all relevant stream names. No pattern will be used as default behavior.
    ///     If this is not valid for the derived class, this method should be overwritten.<br />
    ///     This will only be used, if <see cref="UseAllStreams" /> is set.<br />
    ///     If the returning string is null or empty, the pattern will be ignored.
    /// </summary>
    /// <param name="regEx">The suitable regular expression.</param>
    /// <returns><see langword="true" />, if a valid <see cref="Regex" /> could be retrieved. Otherwise <see langword="false" /></returns>
    protected virtual bool TryGetStreamNamePattern(out Regex regEx)
    {
        regEx = default;

        return false;
    }

    /// <summary>
    ///     Sends the response for the given event to ensure that the underlying saga/process is completed.
    /// </summary>
    /// <param name="sendAction">Async callback that provides an <see cref="IProjectionResponseService"/> to send the response.</param>
    /// <param name="domainEvent">The domain a response will be sent for.</param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task SendResponseAsync(
        Func<IProjectionResponseService, IUserProfileServiceEvent, CancellationToken, Task> sendAction,
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = ServiceProvider.CreateScope();
        var responseService = scope.ServiceProvider.GetRequiredService<IProjectionResponseService>();

        try
        {
            await sendAction.Invoke(responseService, domainEvent, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Could not public response message: {errorMessage}",
                e.Message.AsArgumentList());
        }
    }

    /// <inheritdoc cref="IProjection" />
    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams)
    {
        throw new NotImplementedException("synchronious projections are not supported.");
    }

    /// <inheritdoc cref="IProjection" />
    public async Task ApplyAsync(
        IDocumentOperations operations,
        IReadOnlyList<StreamAction> streams,
        CancellationToken cancellation)
    {
        Logger.ExitMethod();

        if (!UseAllStreams)
        {
            await HandleSingleStreamAsync(streams, cancellation);
        }
        else
        {
            await HandleMultipleStreamAsync(streams, cancellation);
        }

        Logger.ExitMethod();
    }
}
