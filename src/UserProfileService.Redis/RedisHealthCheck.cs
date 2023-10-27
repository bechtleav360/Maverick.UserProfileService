using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Redis;

/// <summary>
///     An implementation of <see cref="IHealthCheck" /> checking a redis connection.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisHealthCheck> _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="RedisHealthCheck" />.
    /// </summary>
    /// <param name="connectionMultiplexer">The <see cref="IConnectionMultiplexer" /> used for the health check.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisHealthCheck> logger)
    {
        _connection = connectionMultiplexer;
        _logger = logger;
    }

    /// <inheritdoc cref="IHealthCheck.CheckHealthAsync" />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _logger.EnterMethod();
        HealthCheckResult result;

        try
        {
            foreach (EndPoint endPoint in _connection.GetEndPoints(true))
            {
                IServer server = _connection.GetServer(endPoint);

                if (server.ServerType != ServerType.Cluster)
                {
                    _logger.LogDebugMessage(
                        "Not connected to a redis cluster, only checking connectivity",
                        LogHelpers.Arguments());

                    await _connection.GetDatabase().PingAsync();
                    await server.PingAsync();
                }
                else
                {
                    _logger.LogDebugMessage("Detected a redis-cluster, checking nodes", LogHelpers.Arguments());
                    RedisResult clusterInfo = await server.ExecuteAsync("CLUSTER", "INFO");

                    if (clusterInfo is { IsNull: false })
                    {
                        // ReSharper disable once PossibleNullReferenceException => clusterInfo.ToString() should not be null as we filtered for IsNull above
                        if (!clusterInfo.ToString().Contains("cluster_state:ok"))
                        {
                            //cluster info is not ok!
                            result = new HealthCheckResult(
                                context.Registration.FailureStatus,
                                $"CLUSTER is not on OK state for endpoint {endPoint}");

                            _logger.LogDebugMessage(
                                "The cluster is not on OK stage for endpoint {endPoint}.",
                                LogHelpers.Arguments(endPoint));

                            return _logger.ExitMethod(result);
                        }
                    }
                    else
                    {
                        //cluster info cannot be read for this cluster node 
                        result = new HealthCheckResult(
                            context.Registration.FailureStatus,
                            $"CLUSTER is null or can't be read for endpoint {endPoint}");

                        _logger.LogDebugMessage(
                            "CLUSTER is null or can't be read for endpoint {endPoint}.",
                            LogHelpers.Arguments(endPoint));

                        return _logger.ExitMethod(result);
                    }
                }
            }

            result = HealthCheckResult.Healthy("Successfully connected to redis");

            return _logger.ExitMethod(result);
        }
        catch (Exception e)
        {
            _logger.LogWarnMessage(
                e,
                "An error occurred while checking the health of redis",
                LogHelpers.Arguments());

            result = new HealthCheckResult(
                context.Registration.FailureStatus,
                "Error while checking health of redis",
                e);

            return _logger.ExitMethod(result);
        }
    }
}
