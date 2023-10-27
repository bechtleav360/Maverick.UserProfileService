using Marten;
using Marten.Events.Daemon;
using Marten.Events.Projections;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Marten.EventStore.Options;

namespace UserProfileService.Marten.EventStore.Implementations;

/// <summary>
///     Implements an <see cref="IHealthCheck" /> checking the Marten event store connectivity.
/// </summary>
public class MartenEventStoreHealthCheck : IHealthCheck
{
    /// <summary>
    ///     The allowed event projection processing lag compared to the HighWaterMark.
    /// </summary>
    private const long MaxEventLag = 500;

    private readonly IOptions<MartenEventStoreOptions> _configuration;
    private readonly ILogger<MartenEventStoreHealthCheck> _logger;
    private readonly IDocumentStore _store;

    /// <summary>
    ///     Create a new instance of <see cref="MartenEventStoreHealthCheck" />
    /// </summary>
    /// <param name="configuration">    Marten event store configuration</param>
    /// <param name="logger">   Provided logger</param>
    /// <param name="store">   Marten document store</param>
    public MartenEventStoreHealthCheck(
        IOptions<MartenEventStoreOptions> configuration,
        ILogger<MartenEventStoreHealthCheck> logger,
        IDocumentStore store)
    {
        _configuration = configuration;
        _logger = logger;
        _store = store;
    }

    /// <inheritdoc cref="IHealthCheck.CheckHealthAsync" />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        const string highWaterMarkDefinition = "HighWaterMark";

        HealthCheckResult result;

        try
        {
            string? streamName = _configuration.Value?.SubscriptionName;

            if (string.IsNullOrWhiteSpace(streamName))
            {
                _logger.LogWarnMessage(
                    "Marten event store configuration is not valid: parameter subscription name should not be null or whitespace",
                    LogHelpers.Arguments());

                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "Marten event store configuration is not valid: parameter subscription name should not be null or whitespace");
            }

            // Part of this has been copied from https://github.com/JasperFx/marten
            // It's the official Marten Github repository
            // The original class was internal and had issues on empty systems
            HashSet<string> projectionsToCheck = _store.Options
                .Events
                .Projections()
                .Where(
                    x => x.Lifecycle
                        == ProjectionLifecycle
                            .Async) // Only check async projections to avoid issues where inline progression counter is set.
                .Select(x => $"{x.ProjectionName}:All")
                .ToHashSet();

            IReadOnlyList<ShardState> allProgress =
                await _store.Advanced.AllProjectionProgress(token: cancellationToken).ConfigureAwait(true);

            // if no progress could be found, no projection has been triggered
            // this can happen on an empty system, where no events has been occurred
            // since this is possible and quite ok, a healthy status can be returned
            //
            // this "if" was not part of the original code and causing problems, if no progress has been found
            if (allProgress.Count == 0)
            {
                _logger.LogDebugMessage("No progress has been found - the system seems empty. Checked {projectionsToCheck} projections.",
                    projectionsToCheck.ToLogString().AsArgumentList());

                return _logger.ExitMethod(
                    HealthCheckResult.Healthy("Connected to Marten event store - all projections empty"));
            }

            ShardState highWaterMark = allProgress.First(x => string.Equals(highWaterMarkDefinition, x.ShardName));

            IEnumerable<ShardState> projectionMarks =
                allProgress.Where(x => !string.Equals(highWaterMarkDefinition, x.ShardName));

            string[] unhealthy = projectionMarks
                .Where(x => projectionsToCheck.Contains(x.ShardName))
                .Where(x => x.Sequence <= highWaterMark.Sequence - MaxEventLag)
                .Select(x => x.ShardName)
                .ToArray();

            result = unhealthy.Length == 0
                ? HealthCheckResult.Healthy("Connected to Marten event store")
                : HealthCheckResult.Degraded(
                    $"Unhealthy: Async projection sequence is more than {MaxEventLag} events behind - unhealthy projection(s): {string.Join(", ", unhealthy)}");
        }
        catch (Exception e)
        {
            _logger.LogWarnMessage(
                e,
                "An error occurred while checking the health of Marten event store",
                LogHelpers.Arguments());

            result = new HealthCheckResult(
                context.Registration.FailureStatus,
                "An unexpected error occurred while checking the health of Marten event store",
                e);
        }

        return _logger.ExitMethod(result);
    }
}
