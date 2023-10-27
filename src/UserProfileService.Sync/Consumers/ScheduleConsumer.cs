using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Messages.Responses;

namespace UserProfileService.Sync.Consumers;

/// <summary>
///     Default consumer for <see cref="IConsumer{SetSyncScheduleCommand}" /> and
///     <see cref="IConsumer{GetSyncScheduleCommand}" />.
/// </summary>
public class ScheduleConsumer
    : IConsumer<SetSyncScheduleCommand>, IConsumer<GetSyncScheduleCommand>
{
    private readonly ILogger<ScheduleConsumer> _logger;
    private readonly IScheduleService _scheduleService;

    /// <summary>
    ///     Create an instance of <see cref="ScheduleConsumer" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="scheduleService">Service to manage sync schedule.</param>
    public ScheduleConsumer(ILogger<ScheduleConsumer> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<SetSyncScheduleCommand> context)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Handle message of type {type} and message content: {content}",
                LogHelpers.Arguments(nameof(SetSyncScheduleCommand), JsonConvert.SerializeObject(context.Message)));
        }

        if (context.Message.IsActive == null)
        {
            await context.Publish(
                new SetSyncScheduleFailure
                {
                    CorrelationId = context.Message.CorrelationId
                },
                context.CancellationToken);

            throw new ArgumentNullException(nameof(context.Message.IsActive), "IsActive can not be null");
        }

        _logger.LogInfoMessage(
            "Handle message of type {type} and set schedule IsActive to {isActive}.",
            LogHelpers.Arguments(nameof(SetSyncScheduleCommand), context.Message.IsActive));

        var request = new ScheduleRequest((bool)context.Message.IsActive);

        try
        {
            await _scheduleService.ChangeScheduleAsync(
                request,
                context.Message.InitiatorId,
                context.CancellationToken);

            _logger.LogInfoMessage(
                "Successful set schedule IsActive to {isActive}.",
                LogHelpers.Arguments(context.Message.IsActive));
        }
        catch (Exception)
        {
            await context.Publish(
                new SetSyncScheduleFailure
                {
                    CorrelationId = context.Message.CorrelationId
                },
                context.CancellationToken);

            _logger.ExitMethod();

            throw;
        }

        // Is not in the try catch, so that the error response is only sent if the write to the database failed.
        await context.Publish(
            new SetSyncScheduleSuccess
            {
                CorrelationId = context.Message.CorrelationId
            },
            context.CancellationToken);

        _logger.LogInfoMessage(
            "Successful handled message of type {type}.",
            LogHelpers.Arguments(nameof(SetSyncScheduleCommand), context.Message.IsActive));

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetSyncScheduleCommand> context)
    {
        _logger.EnterMethod();

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Handle message of type {type} and message content: {content}",
                LogHelpers.Arguments(nameof(GetSyncScheduleCommand), JsonConvert.SerializeObject(context.Message)));
        }

        SyncSchedule schedule = await _scheduleService.GetScheduleAsync(context.CancellationToken);

        await context.Publish(
            new SyncScheduleStatus
            {
                IsActive = schedule.IsActive,
                CorrelationId = context.Message.CorrelationId
            });

        _logger.ExitMethod();
    }
}
