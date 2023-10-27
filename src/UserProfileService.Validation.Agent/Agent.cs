using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.EventCollector.Abstractions.Messages.Responses;
using UserProfileService.EventCollector.Configuration;

namespace UserProfileService.EventCollector;

/// <summary>
///     Defines a class that is a consumer of a message which implements (<see cref="IEventCollectorMessage" />).
///     The agent aggregates the events and sends a composite event.
/// </summary>
/// <typeparam name="TMessage">The message type</typeparam>
/// <typeparam name="TCompositeResponseMessage">The composite message type.</typeparam>
public abstract class Agent<TMessage, TCompositeResponseMessage> : IConsumer<TMessage>
    where TMessage : class, IEventCollectorMessage where TCompositeResponseMessage : class
{
    protected readonly IEventCollectorStore CollectorStore;
    protected readonly EventCollectorConfiguration Configuration;
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected Agent(
        IServiceProvider serviceProvider,
        ILogger logger,
        IOptionsMonitor<EventCollectorConfiguration> eventCollectionConfiguration)
    {
        ServiceProvider = serviceProvider;
        CollectorStore = ServiceProvider.GetRequiredService<IEventCollectorStore>();
        Logger = logger;
        Configuration = eventCollectionConfiguration.CurrentValue;
    }

    protected abstract TCompositeResponseMessage BuildCompositeResponseMessage(
        Guid collectingId,
        ICollection<EventData> eventDataCollection);

    /// <summary>
    ///     Send the composite response containing all collected responses.
    /// </summary>
    /// <param name="collectingId">Id used of the collecting process to send the response for.</param>
    /// <param name="context">
    ///     <see cref="ConsumeContext{T}" />
    /// </param>
    /// <returns> A task representing the asynchronous operation.</returns>
    protected async Task SendCompositeResponseAsync(Guid collectingId, ConsumeContext context)
    {
        Logger.EnterMethod();

        ICollection<EventData> eventCollection =
            await CollectorStore.GetEventData(collectingId.ToString());

        Logger.LogDebugMessage(
            "Found {count} event data in database.",
            eventCollection.Count.AsArgumentList());

        TCompositeResponseMessage compositeResponse =
            BuildCompositeResponseMessage(collectingId, eventCollection);

        await context.Publish(compositeResponse);

        Logger.LogInfoMessage(
            "Set the completion time of the collecting process with the id {requestId}",
            collectingId.AsArgumentList());

        await SetTerminateTimeStampOfCollectingProcessAsync(collectingId, context.CancellationToken);

        Logger.LogInfoMessage(
            "Published composite message for the collecting process with the id {requestId}",
            collectingId.AsArgumentList());

        Logger.ExitMethod();
    }

    /// <summary>
    ///     Set the time stamp when the collecting process is done.
    /// </summary>
    /// <param name="collectingId">The id used to identify the current collecting process. </param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>True if the time stamp has been set, otherwise false</returns>
    protected async Task<bool> SetTerminateTimeStampOfCollectingProcessAsync(
        Guid collectingId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();
        IServiceScope scope = ServiceProvider.CreateScope();
        var collectorStore = scope.ServiceProvider.GetRequiredService<IEventCollectorStore>();

        return Logger.ExitMethod(
            await collectorStore.SetTerminateTimeForCollectingItemsProcessAsync(collectingId, cancellationToken));
    }

    protected virtual Task<StartCollectingEventData> GetStartCollectingEventData(
        ConsumeContext<IEventCollectorMessage> context)
    {
        var startCollectingEventData = new StartCollectingEventData
        {
            CollectingId = context.Message.CollectingId,
            CollectItemsAccount = Configuration.ExpectedResponses
        };

        return Task.FromResult(startCollectingEventData);
    }

    /// <inheritdoc cref="IConsumer{TMessage}" />
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        Logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Message == null)
        {
            throw new ArgumentException("The message can not be null or empty.");
        }

        if (context.Message.CollectingId == Guid.Empty)
        {
            throw new ArgumentException("The collecting id can not be null or empty.");
        }

        EventData eventData = CreateEventData(context.Message, context.Host.MachineName);

        await CollectorStore.SaveEventDataAsync(eventData, context.CancellationToken);

        await CheckAndSendStatus(context);

        Logger.ExitMethod();
    }

    public abstract EventData CreateEventData(TMessage message, string host);

    private protected async Task CheckAndSendStatus<T>(ConsumeContext<T> context)
        where T : class, IEventCollectorMessage
    {
        StartCollectingEventData startCollectingEventData = await GetStartCollectingEventData(context);
        Guid collectionId = context.Message.CollectingId;
        int numberOfEventData = await CollectorStore.GetCountOfEventDataAsync(collectionId.ToString("D"));

        Logger.LogInfoMessage(
            "Found {numberOfEventData} for the collectingId {collectingId}.",
            LogHelpers.Arguments(numberOfEventData, context.Message.CollectingId.ToString()));

        if (startCollectingEventData.CollectItemsAccount == numberOfEventData)
        {
            Logger.LogInfoMessage(
                "All messages were received for the validation process with the id {requestId}",
                context.Message.CollectingId.AsArgumentList());

            await SendCompositeResponseAsync(collectionId, context);
        }
        else if (startCollectingEventData.StatusDispatch == null
                 || startCollectingEventData.StatusDispatch.Modulo == 0)
        {
            Logger.LogWarnMessage(
                "The modulo for sending status messages for collecting id '{id}' is zero/null and must be greater than zero. Sending status messages is ignored.",
                context.Message.CollectingId.AsArgumentList());
        }
        else if (numberOfEventData != 0 && numberOfEventData % startCollectingEventData.StatusDispatch.Modulo == 0)
        {
            await SendStatusAsync(context, numberOfEventData, startCollectingEventData.ExternalProcessId);
        }
    }

    private protected async Task SendStatusAsync<T>(
        ConsumeContext<T> context,
        int? collectedItemsAmount = null,
        string externalProcessId = null)
        where T : class, IEventCollectorMessage
    {
        Guid collectingId = context.Message.CollectingId;

        if (collectedItemsAmount == null)
        {
            Logger.LogInfoMessage(
                "Getting the status of collecing process with the Id {collectingId}.",
                LogHelpers.Arguments(collectingId.ToString()));

            try
            {
                collectedItemsAmount = await CollectorStore.GetCountOfEventDataAsync(
                    collectingId.ToString(),
                    context.CancellationToken);
            }
            catch (Exception)
            {
                Logger.LogWarnMessage(
                    " Error happened by getting the status of collecing process with the Id {collectingId}.",
                    LogHelpers.Arguments(collectingId.ToString()));

                throw;
            }
        }

        if (string.IsNullOrWhiteSpace(externalProcessId))
        {
            Logger.LogInfoMessage(
                "Getting the external process Id of collecting process with the Id {collectingId}.",
                LogHelpers.Arguments(collectingId.ToString()));

            externalProcessId = await CollectorStore.GetExternalProcessIdAsync(
                collectingId.ToString(),
                context.CancellationToken);
        }

        await context.Publish(
            new CollectingItemsStatus
            {
                CollectedItemsAccount = (int)collectedItemsAmount, // can not be null here
                CollectingId = context.Message.CollectingId,
                ExternalProcessId = externalProcessId
            },
            context.CancellationToken);
    }
}

/// <summary>
///     Defines a class that is a consumer of two messages (success and failure) which implements (
///     <see cref="IEventCollectorMessage" />).
///     The agent aggregates the events and sends a composite event.
/// </summary>
/// <typeparam name="TFailureMessage">The message type that used for failure response messages. </typeparam>
/// ///
/// <typeparam name="TSuccessMessage">The message type that used for successful response messages.</typeparam>
/// <typeparam name="TCompositeResponseMessage">The composite message type.</typeparam>
public abstract class
    Agent<TFailureMessage, TSuccessMessage, TCompositeResponseMessage> :
        Agent<TSuccessMessage, TCompositeResponseMessage>,
        IConsumer<TFailureMessage>
    where
    TFailureMessage : class, IEventCollectorMessage
    where
    TSuccessMessage : class, IEventCollectorMessage
    where
    TCompositeResponseMessage : class
{
    protected Agent(
        IServiceProvider serviceProvider,
        ILogger logger,
        IOptionsMonitor<EventCollectorConfiguration> eventCollectionConfiguration) : base(
        serviceProvider,
        logger,
        eventCollectionConfiguration)
    {
    }

    /// <inheritdoc cref="IConsumer{TFailureMessage}" />
    public async Task Consume(ConsumeContext<TFailureMessage> context)
    {
        Logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Message == null)
        {
            throw new ArgumentException("The message can not be null or empty.");
        }

        if (context.Message.CollectingId == Guid.Empty)
        {
            throw new ArgumentException("The collecting id should not be empty.");
        }

        EventData eventData = CreateEventData(context.Message, context.Host.MachineName);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTrace(
                "Saving the eventData: {eventData} for the collectingId: {collectiondId}",
                LogHelpers.Arguments(eventData.ToLogString(), context.Message.CollectingId.ToLogString()));
        }

        await CollectorStore.SaveEventDataAsync(eventData, context.CancellationToken);

        Logger.LogInfoMessage(
            "Saved events consolidated in database for request {requestId} incl. current response from sender {sender}",
            LogHelpers.Arguments(context.Message.CollectingId, context.Host.MachineName));

        await CheckAndSendStatus(context);

        Logger.ExitMethod();
    }

    public abstract EventData CreateEventData(TFailureMessage message, string host);
}
