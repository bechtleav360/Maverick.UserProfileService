using System;
using MassTransit;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Configuration;

/// <summary>
///     Options for arango saga repository for <see cref="TSaga" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga.</typeparam>
public class ArangoSagaRepositoryOptions<TSaga>
    where TSaga : class, ISaga
{
    /// <summary>
    ///     Name to use for client factory.
    /// </summary>
    public string ClientName { get; }

    /// <summary>
    ///     Name of collection to use for saga.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    ///     Concurrency mode to save saga.
    /// </summary>
    public ConcurrencyMode ConcurrencyMode { get; }

    /// <summary>
    ///     Prefix to use for saga key.
    /// </summary>
    public string KeyPrefix { get; }

    /// <summary>
    ///     Policy that defines the repetition to save the saga.
    /// </summary>
    public IRetryPolicy RetryPolicy { get; }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryOptions{TSaga}" />.
    /// </summary>
    /// <param name="concurrencyMode">Concurrency mode to save saga.</param>
    /// <param name="keyPrefix">Prefix to use for saga key.</param>
    /// <param name="collectionName">Name of collection to use for saga.</param>
    /// <param name="clientName">Name of arango client.</param>
    public ArangoSagaRepositoryOptions(
        ConcurrencyMode concurrencyMode,
        string keyPrefix,
        string collectionName,
        string clientName)
    {
        ConcurrencyMode = concurrencyMode;

        KeyPrefix = string.IsNullOrWhiteSpace(keyPrefix)
            ? null
            : keyPrefix.EndsWith(":")
                ? keyPrefix
                : $"{keyPrefix}:";

        RetryPolicy = Retry.Exponential(
            10,
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(918));

        CollectionName = collectionName;
        ClientName = clientName;
    }

    /// <summary>
    ///     Creates the proper saga key from the configured options.
    /// </summary>
    /// <param name="correlationId">Identifier to create the saga key.</param>
    /// <returns>Proper saga key for repository.</returns>
    public string FormatSagaKey(Guid correlationId)
    {
        return KeyPrefix != null ? $"{KeyPrefix}{correlationId}" : correlationId.ToString();
    }
}
