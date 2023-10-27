using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UserProfileService.Redis.Configuration;

namespace UserProfileService.Redis;

/// <summary>
///     The redis cache store contains methods to store key values pairs in the
///     redis.
/// </summary>
public class RedisCacheStore : RedisStoreBase
{
    /// <summary>
    ///     Create an instance of <see cref="RedisTempObjectStore" />.
    /// </summary>
    /// <param name="connectionMultiplexer">
    ///     <see cref="IConnectionMultiplexer" />
    /// </param>
    /// <param name="redisOptions">Options to use to configure redis and key-value handling.</param>
    /// <param name="logger">The logger.</param>
    public RedisCacheStore(
        IConnectionMultiplexer connectionMultiplexer,
        IOptionsMonitor<RedisConfiguration> redisOptions,
        ILogger<RedisTempObjectStore> logger)
        : base(connectionMultiplexer, redisOptions, logger)
    {
    }
}
