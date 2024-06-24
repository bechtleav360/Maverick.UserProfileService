using System.Collections.Generic;
using System.Security.Authentication;
using StackExchange.Redis;

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
    ///     Optional channel prefix for all pub/sub operations
    /// </summary>
    public string ChannelPrefix { get; set; } = null;

    /// <summary>
    ///     A Boolean value that specifies whether the certificate revocation list is checked during authentication.
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = true;

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
    ///     Broadcast channel name for communicating configuration changes
    /// </summary>
    public string ConfigurationChannel { get; set; } = "__Booksleeve_MasterChanged";

    /// <summary>
    ///     Time (seconds) to check configuration. This serves as a keep-alive for interactive sockets, if it is supported.
    /// </summary>
    public int ConfigCheckSeconds { get; set; } = 60;

    /// <summary>
    ///     Default database index, from 0 to databases - 1
    /// </summary>
    public int? DefaultDatabase { get; set; } = null;

    /// <summary>
    ///     Time (seconds) at which to send a message to help keep sockets alive (60 sec default)
    /// </summary>
    public int KeepAlive { get; set; } = -1;

    /// <summary>
    ///     Identification for the connection within redis
    /// </summary>
    public string ClientName { get; set; } = null;
    
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
    
    /// <summary>
    /// Type of proxy in use (if any); for example “twemproxy/envoyproxy”
    /// </summary>
    public Proxy Proxy { get; set; } = Proxy.None;
    
    /// <summary>
    ///     Specifies that DNS resolution should be explicit and eager, rather than implicit
    /// </summary>
    public bool ResolveDns { get; set; } = false;
    
    
    /// <summary>
    ///     The service name used to resolve a service via sentinel.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the Redis connection uses SSL.
    /// </summary>
    /// <value>
    ///     <c>true</c> if SSL is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool Ssl { get; set; }

    /// <summary>
    ///     Enforces a particular SSL host identity on the server’s certificate
    /// </summary>
    public string SslHost { get; set; }

    /// <summary>
    ///     Ssl/Tls versions supported when using an encrypted connection. Use ‘|’ to provide multiple values.
    /// </summary>
    public SslProtocols? SslProtocols { get; set; }

    /// <summary>
    ///     Time (ms) to allow for synchronous operations
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Time (ms) to allow for asynchronous operations
    /// </summary>
    public int AsyncTimeout { get; set; } = 5000;

    /// <summary>
    /// 	Key to use for selecting a server in an ambiguous primary scenario.
    /// </summary>
    public string TieBreaker { get; set; } = "__Booksleeve_TieBreak";

    /// <summary>
    ///     Whether to attempt to use CLIENT SETINFO to set the library name/version on the connection
    /// </summary>
    public bool SetClientLibrary { get; set; } = true;

    /// <summary>
    ///     Redis protocol to use; see section below
    /// </summary>
    public RedisProtocol? Protocol { get; set; }
}
