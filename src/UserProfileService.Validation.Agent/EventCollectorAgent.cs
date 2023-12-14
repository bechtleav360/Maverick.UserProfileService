using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.EventCollector.Abstractions.Messages.Responses;
using UserProfileService.EventCollector.Configuration;

namespace UserProfileService.EventCollector;

/// <summary>
///     An implementation of <see cref="Agent{TMessage,TCompositeResponseMessage}" /> used to collect event of type
///     <see cref="SubmitCommandResponseMessage" />
///     an return their as bundle corresponding to <see cref="CollectingItemsResponse{TSuccess,TFailure}" />
/// </summary>
public class EventCollectorAgent :
    Agent<SubmitCommandSuccess, SubmitCommandFailure,
        CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>>,
    IConsumer<StartCollectingMessage>,
    IConsumer<SetCollectItemsAccountMessage>,
    IConsumer<GetCollectingItemsStatusMessage>
{
    /// <summary>
    ///     Creates a new instance of <see cref="EventCollectorAgent" />
    /// </summary>
    /// <param name="serviceProvider"> The service provider used to resolved services for the event collector</param>
    /// <param name="logger"> The logger <see cref="ILogger" /></param>
    /// <param name="eventCollectionConfiguration"> The configuration of the event collector.</param>
    public EventCollectorAgent(
        IServiceProvider serviceProvider,
        ILogger<EventCollectorAgent> logger,
        IOptionsMonitor<EventCollectorConfiguration> eventCollectionConfiguration) : base(
        serviceProvider,
        logger,
        eventCollectionConfiguration)
    {
    }

    private EventData CreateEventDataInternal<TSubmitCommand>(TSubmitCommand message, string host)
        where TSubmitCommand : SubmitCommandResponseMessage
    {
        Guid collectingId = message.CollectingId;
        string requestId = message.Id.Id;

        try
        {
            return new EventData
            {
                CollectingId = collectingId,
                Data = JsonSerializer.Serialize(message),
                ErrorOccurred = message.ErrorOccurred,
                RequestId = requestId,
                Host = host
            };
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "An error occurred while handling message for process {collectingId}, request {requestId} and host {host}.",
                LogHelpers.Arguments(collectingId, requestId, host));

            throw;
        }
    }

    /// <inheritdoc />
    protected override CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>
        BuildCompositeResponseMessage(
            Guid collectingId,
            ICollection<EventData> eventDataCollection)
    {
        Logger.EnterMethod();

        var successResponses = new List<SubmitCommandSuccess>();
        var failureResponses = new List<SubmitCommandFailure>();

        Logger.LogDebugMessage(
            $"Getting external process id from collecting process with id {collectingId}.",
            LogHelpers.Arguments(collectingId));

        string externalProcessId = CollectorStore.GetExternalProcessIdAsync(collectingId.ToString())
            .GetAwaiter()
            .GetResult();

        if (eventDataCollection == null || !eventDataCollection.Any())
        {
            return new CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>
            {
                CollectingId = collectingId,
                ExternalProcessId = externalProcessId
            };
        }

        foreach (EventData eventData in eventDataCollection)
        {
            if (eventData == null)
            {
                Logger.LogWarnMessage(
                    "Event data should not be null while processing collecting result.",
                    LogHelpers.Arguments());

                continue;
            }

            try
            {
                if (eventData.ErrorOccurred)
                {
                    var data = JsonSerializer.Deserialize<SubmitCommandFailure>(eventData.Data);

                    if (data == null)
                    {
                        Logger.LogWarnMessage(
                            "Data should not be null while processing collecting result of host {host} and collecting id {collectingId} with request id {requestId}. Event data will be skipped.",
                            LogHelpers.Arguments(eventData.Host, eventData.CollectingId, eventData.RequestId));

                        continue;
                    }

                    failureResponses.Add(data);
                }
                else
                {
                    var data = JsonSerializer.Deserialize<SubmitCommandSuccess>(eventData.Data);

                    if (data == null)
                    {
                        Logger.LogWarnMessage(
                            "Data should not be null while processing collecting result of host {host} and collecting id {collectingId}  with request id {requestId}. Event data will be skipped.",
                            LogHelpers.Arguments(eventData.Host, eventData.CollectingId, eventData.RequestId));

                        continue;
                    }

                    successResponses.Add(data);
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"An error occurred while processing composite of event responses of host {eventData.Host} and collecting id {eventData.CollectingId} for request {eventData.RequestId}.",
                    e);
            }
        }

        Logger.ExitMethod();

        return new CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>
        {
            CollectingId = collectingId,
            Failures = failureResponses,
            Successes = successResponses,
            ExternalProcessId = externalProcessId
        };
    }

    protected override async Task<StartCollectingEventData> GetStartCollectingEventData(
        ConsumeContext<IEventCollectorMessage> context)
    {
        Logger.EnterMethod();

        var startCollectingEventData = await CollectorStore.GetEntityAsync<StartCollectingEventData>(
            context.Message.CollectingId.ToString(),
            context.CancellationToken);

        return Logger.ExitMethod(startCollectingEventData);
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<StartCollectingMessage> context)
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
            throw new ArgumentException("The process id can not be empty.");
        }

        int? collectingAmount = context.Message.CollectItemsAccount;

        if (collectingAmount != null && collectingAmount.Value <= 0)
        {
            throw new ArgumentException("The collecting item amount should not be less than 0 .");
        }

        var eventData = new StartCollectingEventData
        {
            CollectItemsAccount = context.Message.CollectItemsAccount,
            StartedAt = DateTime.UtcNow,
            CollectingId = context.Message.CollectingId,
            ExternalProcessId = context.Message.ExternalProcessId
        };

        try
        {
            await CollectorStore.SaveEntityAsync(
                eventData,
                eventData.CollectingId.ToString());

            await context.Publish(
                new StartCollectingEventSuccess
                {
                    CollectingId = eventData.CollectingId,
                    ExternalProcessId = context.Message.ExternalProcessId
                });

            Logger.LogInfoMessage(
                "Successful start collecting event request with the collecting Id {collectingId}.",
                LogHelpers.Arguments(eventData.CollectingId));
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error occurred by starting collecting event request with the collecting Id {collectingId}.",
                LogHelpers.Arguments(eventData.CollectingId));

            await context.Publish(
                new StartCollectingEventFailure
                {
                    ErrorMessage = e.Message,
                    CollectingId = context.Message.CollectingId,
                    ExternalProcessId = context.Message.ExternalProcessId
                });

            Logger.ExitMethod();

            throw;
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<SetCollectItemsAccountMessage> context)
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
            throw new ArgumentException("The process id can not be null or empty.");
        }

        int collectingAmount = context.Message.CollectItemsAccount;

        if (collectingAmount <= 0)
        {
            throw new ArgumentException("The collecting item amount should not be less than 0 .");
        }

        Guid collectingId = context.Message.CollectingId;
        int amountOfItems = context.Message.CollectItemsAccount;

        Logger.LogInfoMessage(
            "Trying to update the amount of items to collect: collectingId: {collectingId}, amount of Items: {amount}",
            LogHelpers.Arguments(collectingId.ToString(), amountOfItems));

        bool amountUpdated = await CollectorStore.TrySetCollectingItemsAmountAsync(
            context.Message.CollectingId,
            context.Message.CollectItemsAccount,
            context.CancellationToken);

        if (amountUpdated)
        {
            Logger.LogInfoMessage(
                "The number of events that should be collected for the batch with collecting Id {collectingId} has been set {collectingAmount}.",
                LogHelpers.Arguments(collectingId, amountOfItems));
        }
        else
        {
            Logger.LogInfoMessage(
                "The number of events that should be collected for the batch with collecting Id {collectingId} was already set und can't be actualized.",
                LogHelpers.Arguments(collectingId));
        }

        await SendStatusAsync(context);

        int collectedItemsAmount = await CollectorStore.GetCountOfEventDataAsync(
            context.Message.CollectingId.ToString(),
            context.CancellationToken);

        // If the number of already collected items is equal to the total number of items, a composite response is sent out.
        if (amountOfItems == collectedItemsAmount)
        {
            await SendCompositeResponseAsync(collectingId, context);
        }

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetCollectingItemsStatusMessage> context)
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

        await SendStatusAsync(context);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public override EventData CreateEventData(SubmitCommandSuccess message, string host)
    {
        Logger.EnterMethod();

        return Logger.ExitMethod(CreateEventDataInternal(message, host));
    }

    /// <inheritdoc />
    public override EventData CreateEventData(SubmitCommandFailure message, string host)
    {
        Logger.EnterMethod();

        return Logger.ExitMethod(CreateEventDataInternal(message, host));
    }
}
