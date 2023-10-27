using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Sync.Abstraction.Stores;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Responses;

namespace UserProfileService.Sync.Services;

/// <summary>
///     The default implementation of <see cref="IScheduleService" />.
/// </summary>
public class ScheduleService : IScheduleService
{
    private readonly ILogger<ScheduleService> _logger;
    private readonly IBus _messageBus;
    private readonly IScheduleStore _scheduleStore;

    /// <summary>
    ///     Create an instance of <see cref="SynchronizationService" />.
    /// </summary>
    /// <param name="scheduleStore">Component that communicates with the configured message broker.</param>
    /// <param name="messageBus">Bus to publish messages.</param>
    /// <param name="logger">The logger for logging purposes.</param>
    public ScheduleService(
        IScheduleStore scheduleStore,
        IBus messageBus,
        ILogger<ScheduleService> logger)
    {
        _scheduleStore = scheduleStore;
        _messageBus = messageBus;
        _logger = logger;
    }

    private async Task<bool> TryToSendStatusAsync(SyncSchedule schedule, CancellationToken cancellationToken)
    {
        try
        {
            var currentSchedule =
                new SyncScheduleStatus
                {
                    CorrelationId = Guid.NewGuid(),
                    IsActive = schedule.IsActive
                };

            await _messageBus.Publish(currentSchedule, cancellationToken);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogErrorMessage(
                e,
                "An error occurred while sending current schedule to message broker. Error is ignored in order not to interrupt the further process of the request.",
                LogHelpers.Arguments());

            return false;
        }
    }

    /// <inheritdoc />
    public async Task<SyncSchedule> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        SyncSchedule schedule = await _scheduleStore.GetScheduleAsync(cancellationToken);

        if (schedule == null)
        {
            _logger.LogInfoMessage(
                "No schedule could be found. Default schedule with IsActive = true will be returned.",
                LogHelpers.Arguments());

            return new SyncSchedule
            {
                IsActive = true,
                ModifiedAt = DateTime.UtcNow
            };
        }

        _logger.LogInfoMessage(
            "Found schedule with status {status} and modified at {modifiedAt}",
            LogHelpers.Arguments(schedule.IsActive, schedule.ModifiedAt));

        return _logger.ExitMethod(schedule);
    }

    /// <inheritdoc />
    public async Task<SyncSchedule> ChangeScheduleAsync(
        ScheduleRequest schedule,
        string userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (schedule == null)
        {
            throw new ArgumentNullException(nameof(schedule));
        }
        
        var syncSchedule = new SyncSchedule
        {
            IsActive = schedule.IsActive,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = userId
        };

        _logger.LogDebugMessage("Set schedule of synchronization to {isActive}", schedule.IsActive.AsArgumentList());

        syncSchedule = await _scheduleStore.SaveScheduleAsync(syncSchedule, cancellationToken);

        await TryToSendStatusAsync(syncSchedule, cancellationToken);

        return _logger.ExitMethod(syncSchedule);
    }
}
