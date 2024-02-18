using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Messages;
using UserProfileService.Sync.States.Messages;
using UserProfileService.Sync.Abstraction.Configurations;
using Microsoft.Extensions.Options;

namespace UserProfileService.Sync.Consumers;

/// <summary>
///     Default consumer for <see cref="IConsumer{StartStopWatchCommand}" />
/// </summary>
public class TriggerStopWatchConsumer : IConsumer<TriggerStopWatchCommand>
{
    private readonly ILogger<TriggerStopWatchConsumer> _logger;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates a new instance of <see cref="TriggerStopWatchConsumer"/>
    /// </summary>
    ///  /// <param name="syncConfigOptions"> Options to configure synchronization. </param>
    /// <param name="logger"></param>
    public TriggerStopWatchConsumer(
        IOptions<SyncConfiguration> syncConfigOptions,
        ILogger<TriggerStopWatchConsumer> logger)
    {
        _logger = logger;
        _syncConfiguration = syncConfigOptions.Value;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<TriggerStopWatchCommand> context)
    {
        _logger.EnterMethod();

        await Task.Delay(TimeSpan.FromMinutes(_syncConfiguration.DelayBeforeTimeoutForStep));

        _logger.LogInfoMessage(
            "The collecting with id: {id} related to correlation id: {corrId} will be skipped by state machine - timeout",
            LogHelpers.Arguments(context.Message.CollectingId, context.CorrelationId));

        await context.Publish(
            new SkipStepMessage
            {
                Id = context.CorrelationId.GetValueOrDefault(),
                CollectingId = context.Message.CollectingId
            });

        _logger.ExitMethod();
    }
}
