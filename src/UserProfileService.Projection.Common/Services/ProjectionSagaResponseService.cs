using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Commands;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.Common.Services;

internal class ProjectionSagaResponseService : IProjectionResponseService
{
    private readonly ILogger<ProjectionSagaResponseService> _Logger;
    private readonly IBus _MessageBus;

    /// <summary>
    ///     Create an instance of <see cref="ProjectionSagaResponseService" />
    /// </summary>
    /// <param name="logger">the Logger.</param>
    /// <param name="messageBus">Component that communicates with the configured message broker.</param>
    public ProjectionSagaResponseService(
        ILogger<ProjectionSagaResponseService> logger,
        IBus messageBus)
    {
        _Logger = logger;
        _MessageBus = messageBus;
    }

    private async Task TryExecuteResponseMessageAsync<TMessage>(
        IUserProfileServiceEvent domainEvent,
        TMessage response,
        int times = 2)
        where TMessage : ICommand
    {
        _Logger.EnterMethod();

        // Only when all events of the batch have been processed or none batch is set, a response is sent to the saga service.
        if (domainEvent.MetaData.Batch != null
            && domainEvent.MetaData.Batch.Current < domainEvent.MetaData.Batch.Total)
        {
            _Logger.LogDebugMessage(
                "{current} of {total} events were processed. No response is sent to the saga service yet.",
                LogHelpers.Arguments(domainEvent.MetaData.Batch.Current, domainEvent.MetaData.Batch.Total));

            return;
        }

        _Logger.LogDebugMessage(
            "All of {total} events were processed. Response will be sent to the saga service.",
            LogHelpers.Arguments(domainEvent.MetaData.Batch?.Total.ToString() ?? "unknown"));

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Try to publish response message to message broker with data: {data}",
                JsonConvert.SerializeObject(response).AsArgumentList());
        }

        // Tries to send the message three times
        var retryCount = 0;

        while (retryCount <= times)
        {
            _Logger.LogDebugMessage("Try to publish response message to message broker.", LogHelpers.Arguments());

            try
            {
                await _MessageBus.Publish(response);

                _Logger.LogDebugMessage(
                    "Successful published response message to message broker.",
                    LogHelpers.Arguments());

                return;
            }
            catch (Exception e)
            {
                if (retryCount == times)
                {
                    _Logger.LogErrorMessage(
                        e,
                        "An error occurred while sending response message to message broker. All retries failed",
                        LogHelpers.Arguments());

                    throw;
                }

                retryCount++;
            }
            finally
            {
                _Logger.ExitMethod();
            }
        }
    }

    /// <inheritdoc />
    public async Task ResponseAsync(
        IUserProfileServiceEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent), "Domain event can not be null");
        }

        if (domainEvent.MetaData == null)
        {
            throw new ArgumentNullException(
                nameof(domainEvent.MetaData),
                "Metadata in domain event can not be null");
        }

        var messagePayload = new CommandProjectionSuccess(domainEvent.MetaData.ProcessId);
        await TryExecuteResponseMessageAsync(domainEvent, messagePayload);

        _Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task ResponseAsync(
        IUserProfileServiceEvent domainEvent,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent), "Domain event can not be null");
        }

        var commandResponse = new CommandProjectionFailure(
            domainEvent.MetaData.ProcessId,
            exception.Message,
            exception);

        await TryExecuteResponseMessageAsync(domainEvent, commandResponse);

        _Logger.ExitMethod();
    }
}
