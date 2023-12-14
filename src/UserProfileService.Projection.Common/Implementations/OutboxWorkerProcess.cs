using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.Configurations;

namespace UserProfileService.Projection.Common.Implementations;

internal class OutboxWorkerProcess : BackgroundService
{
    /// <summary>
    ///     The activity source wrapper to be used for this instance.
    /// </summary>
    private readonly IActivitySourceWrapper _ActivitySourceWrapper;

    private CancellationTokenSource _CancellationTokenSource;

    private readonly IDisposable _ChangeToken;

    private readonly ILogger<OutboxWorkerProcess> _Logger;
    private readonly IOutboxProcessorService _OutboxProcessor;

    /// <summary>
    ///     Gets the configuration of outbox processor.
    /// </summary>
    private OutboxConfiguration OutboxConfiguration { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="OutboxWorkerProcess" />
    /// </summary>
    /// <param name="logger">Logger that will accept logging messages.</param>
    /// <param name="activitySourceWrapper">Wrapper to wrap the activity source to use dependency injection.</param>
    /// <param name="outboxProcessor">Service to use to process outbox table.</param>
    /// <param name="outboxConfiguration">Configuration for outbox processor.</param>
    public OutboxWorkerProcess(
        ILogger<OutboxWorkerProcess> logger,
        IActivitySourceWrapper activitySourceWrapper,
        IOutboxProcessorService outboxProcessor,
        IOptionsMonitor<OutboxConfiguration> outboxConfiguration)
    {
        _Logger = logger;
        _ActivitySourceWrapper = activitySourceWrapper;
        _OutboxProcessor = outboxProcessor;

        _ChangeToken = outboxConfiguration.OnChange(OnConfigurationChanged);
    }

    private void OnConfigurationChanged(OutboxConfiguration newConfiguration)
    {
        if (newConfiguration == null
            || newConfiguration.Equals(OutboxConfiguration))
        {
            return;
        }

        OutboxConfiguration = newConfiguration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _Logger.EnterMethod();

        _CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Activity activity = _ActivitySourceWrapper.ActivitySource.StartActivity(
                $"{GetType().Name}: Check and process outbox");

            _Logger.LogDebugMessage("Start checking outbox to execute batches,", LogHelpers.Arguments());

            await _OutboxProcessor.CheckAndProcessEvents(_CancellationTokenSource.Token);

            // wait 5 seconds as default
            await Task.Delay(
                OutboxConfiguration?.Time ?? TimeSpan.FromSeconds(5),
                _CancellationTokenSource.Token);

            activity?.Stop();
        }
    }

    /// <summary>
    ///     Performs a closing and cleanup for all resources in a safe way.
    /// </summary>
    /// <param name="disposing">Is true, if dispose has already been triggered.</param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ChangeToken?.Dispose();
            _CancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }

    /// <inheritdoc />
    public sealed override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
