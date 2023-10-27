using System.Collections.Generic;

namespace UserProfileService.Redis.Configuration;

/// <summary>
///     Definition to configure redis.
/// </summary>
public class RedisConfiguration
{
    /// <summary>
    ///     Connect will not create a connection while no servers are available.
    /// </summary>
    public bool AbortOnConnectFail { get; set; }

    /// <summary>
    ///     Enables a range of commands that are considered risky.
    /// </summary>
    public bool AllowAdmin { get; set; }

    /// <summary>
    ///     The number of times to repeat connect attempts during initial connect.
    ///     Default are three retries.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    ///     Timeout (ms) for connect operations.
    ///     Default is 5000ms.
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    ///     Connection urls for redis instances.
    /// </summary>
    public List<string> EndpointUrls { get; set; }

    /// <summary>
    ///     Expiration time after the stored value in redis expires and is deleted (in seconds).
    ///     Default is 60 sec (10min).
    /// </summary>
    public int ExpirationTime { get; set; } = 600;

    /// <summary>
    ///     Password for the redis server.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     User for the redis server (for use with ACLs on redis 6 and above).
    /// </summary>
    public string User { get; set; }
}
