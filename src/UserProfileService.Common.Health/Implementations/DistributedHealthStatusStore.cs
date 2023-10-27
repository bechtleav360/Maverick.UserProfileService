using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.Health.Implementations;

/// <summary>
///     An implementation of <see cref="IDistributedHealthStatusStore" /> utilizing a list to store incoming states.
/// </summary>
public class DistributedHealthStatusStore : IDistributedHealthStatusStore
{
    private readonly IList<HealthCheckMessage> _healthMessages;
    private readonly ILogger<DistributedHealthStatusStore> _logger;
    private readonly ISet<string> _workers;

    /// <summary>
    ///     Initializes a new <see cref="DistributedHealthStatusStore" />.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    public DistributedHealthStatusStore(ILogger<DistributedHealthStatusStore> logger)
    {
        _logger = logger;
        _healthMessages = new SynchronizedCollection<HealthCheckMessage>();
    }

    /// <summary>
    ///     Initializes a new <see cref="DistributedHealthStatusStore" />.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="workers">A set of worker names to filter for. Set to <see langword="null" /> to listen to any worker.</param>
    public DistributedHealthStatusStore(ILogger<DistributedHealthStatusStore> logger, params string[] workers) : this(
        logger)
    {
        _workers = workers?.ToHashSet();
    }

    /// <inheritdoc />
    public Task AddHealthStatusAsync(HealthCheckMessage healthStatus, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (healthStatus == null)
        {
            throw new ArgumentNullException(nameof(healthStatus));
        }

        if (_workers is not null
            && !_workers.Contains(healthStatus.WorkerName))
        {
            // skip message
            return Task.CompletedTask;
        }

        List<HealthCheckMessage> oldStatus = _healthMessages.Where(
                s =>
                    s.WorkerName == healthStatus.WorkerName && s.InstanceName == healthStatus.InstanceName)
            .ToList();

        oldStatus.ForEach(m => _healthMessages.Remove(m));

        _healthMessages.Add(healthStatus);

        _logger.ExitMethod();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> GetHealthStatusAsync(
        string workerName,
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        List<HealthCheckMessage> instances = _healthMessages
            .Where(m => m.Time > DateTime.UtcNow && m.WorkerName == workerName)
            .OrderByDescending(m => m.Status)
            .ToList();

        string description = "Health Status of " + workerName;

        IDictionary<string, object> data = new Dictionary<string, object>
        {
            { "Healthy", instances.Count(m => m.Status == HealthStatus.Healthy) },
            { "Degraded", instances.Count(m => m.Status == HealthStatus.Degraded) },
            { "Unhealthy", instances.Count(m => m.Status == HealthStatus.Unhealthy) }
        };

        HealthStatus status = failureStatus;

        if (instances.Any())
        {
            // the first one is the most positive health check because the list is sorted.
            HealthCheckMessage check = instances.First();
            data.Add(nameof(HealthCheckMessage.Version), check.Version);
            status = check.Status;
        }

        if (status < failureStatus)
        {
            status = failureStatus;
        }

        var response = new HealthCheckResult(
            status,
            description,
            data: new ReadOnlyDictionary<string, object>(data));

        _logger.ExitMethod(response);

        return Task.FromResult(response);
    }

    /// <inheritdoc />
    public Task CleanupAsync()
    {
        _logger.EnterMethod();

        List<HealthCheckMessage> oldStatus = _healthMessages.Where(m => m.Time <= DateTime.UtcNow)
            .ToList();

        _logger.LogDebugMessage("Removing {N} unnecessary health reports", new object[] { oldStatus.Count });
        oldStatus.ForEach(m => _healthMessages.Remove(m));

        _logger.ExitMethod();

        return Task.CompletedTask;
    }
}
