using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages;
using UserProfileService.Sync.Messages.Responses;

namespace UserProfileService.Sync.Consumers;

/// <summary>
///     Default consumer for <see cref="IConsumer{SetSyncStatus}" />
/// </summary>
public class GetSyncStatusConsumer : IConsumer<GetSyncStatus>
{
    private readonly ILogger<GetSyncStatusConsumer> _logger;

    private readonly ISynchronizationService _synchronizationService;

    /// <summary>
    ///     Creates an instance of <see cref="GetSyncStatusConsumer" />
    /// </summary>
    /// <param name="synchronizationService"> An instance of <see cref="ISynchronizationService" /></param>
    /// <param name="logger"> An instance of <see cref="ILogger" /></param>
    public GetSyncStatusConsumer(
        ISynchronizationService synchronizationService,
        ILogger<GetSyncStatusConsumer> logger)
    {
        _synchronizationService = synchronizationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetSyncStatus> context)
    {
        _logger.EnterMethod();

        string requestId = context.Message.RequestId;

        _logger.LogInfoMessage(
            "Consuming Get sync status event, with request Id: {id}",
            LogHelpers.Arguments(requestId));

        SyncStatus status = await _synchronizationService.GetSyncStatusAsync(requestId, context.CancellationToken);

        _logger.LogInfoMessage(
            "Sending status (UPS-Sync status) event, with request Id: {id}",
            LogHelpers.Arguments(requestId));

        await context.Publish(status);

        _logger.ExitMethod();
    }
}
