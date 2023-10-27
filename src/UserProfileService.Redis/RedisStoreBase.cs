using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Redis.Configuration;

namespace UserProfileService.Redis;

/// <summary>
///     Redis base implementation of <see cref="ICacheStore" />
/// </summary>
public abstract class RedisStoreBase : ICacheStore
{
    /// <summary>
    ///     The time span when the value should be expire in the redis cache.
    /// </summary>
    protected TimeSpan ConfiguredExpirationTime { get; }

    /// <summary>
    ///     The logger that is used to write message with different severities.
    /// </summary>
    protected ILogger<RedisTempObjectStore> Logger { get; }

    /// <summary>
    ///     The store that is used to store key value pairs.
    /// </summary>
    protected IDatabase Store { get; }

    /// <summary>
    ///     Create an instance of <see cref="RedisTempObjectStore" />.
    /// </summary>
    /// <param name="connectionMultiplexer">
    ///     <see cref="IConnectionMultiplexer" />
    /// </param>
    /// <param name="redisOptions">Options to use to configure redis and key-value handling.</param>
    /// <param name="logger">The logger.</param>
    protected RedisStoreBase(
        IConnectionMultiplexer connectionMultiplexer,
        IOptionsMonitor<RedisConfiguration> redisOptions,
        ILogger<RedisTempObjectStore> logger)
    {
        Store = connectionMultiplexer.GetDatabase();
        ConfiguredExpirationTime = TimeSpan.FromSeconds(redisOptions.CurrentValue.ExpirationTime);
        Logger = logger;
    }

    /// <summary>
    ///     Deserialize JSON string without throwing an exception, even if an error occurs.
    /// </summary>
    /// <remarks>
    ///     A logging message with error information will be send.
    /// </remarks>
    protected T DeserializeSafely<T>(
        string json,
        JsonSerializerSettings settings = null,
        [CallerMemberName] string caller = null)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch (Exception e)
        {
            Logger.LogInfoMessage(
                "Error occurred during deserializing JSON string. Caller: {caller}. Error message: {error}",
                Arguments(caller, e.Message));

            return default;
        }
    }

    /// <summary>
    ///     Returns arguments list as array of objects.
    /// </summary>
    protected object[] Arguments(params object[] objects)
    {
        return objects;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T obj,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSettingsProvider = default,
        CancellationToken token = default)
    {
        string objStr = jsonSettingsProvider?.GetNewtonsoftSettings() != null
            ? JsonConvert.SerializeObject(obj, jsonSettingsProvider.GetNewtonsoftSettings())
            : JsonConvert.SerializeObject(obj);

        TimeSpan expires = expirationTime == 0 ? ConfiguredExpirationTime : TimeSpan.FromSeconds(expirationTime);

        if (transaction is RedisTransaction { Payload: ITransaction tObj } redisTransaction)
        {
            redisTransaction.Operations.Add(tObj.StringSetAsync(key, objStr, expires));
        }
        else
        {
            await Store.StringSetAsync(key, objStr, expires);
        }

        Logger.LogDebugMessage("SetAsync key {key} in store.", LogHelpers.Arguments(key));
    }

    /// <inheritdoc />
    public async Task<T> GetAsync<T>(
        string key,
        IJsonSerializerSettingsProvider jsonSettingsProvider = default,
        CancellationToken token = default)
    {
        try
        {
            string value = await Store.StringGetAsync(key);
            Logger.LogDebugMessage("GetAsync key {key}", LogHelpers.Arguments(key));

            if (value != null)
            {
                return jsonSettingsProvider?.GetNewtonsoftSettings() != null
                    ? JsonConvert.DeserializeObject<T>(value, jsonSettingsProvider.GetNewtonsoftSettings())
                    : JsonConvert.DeserializeObject<T>(value);
            }
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(ex, "An error occurred while getting key {key}.", LogHelpers.Arguments(key));
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<Tuple<bool, string>> LockAsync(
        string key,
        int expirationTime = 0,
        CancellationToken token = default)
    {
        try
        {
            var lockId = Guid.NewGuid().ToString();

            TimeSpan expires =
                expirationTime == 0 ? ConfiguredExpirationTime : TimeSpan.FromSeconds(expirationTime);

            bool @lock = await Store.LockTakeAsync(key, lockId, expires);

            return new Tuple<bool, string>(@lock, lockId);
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while lock key {key} with expiration time {expirationTime}.",
                LogHelpers.Arguments(key, expirationTime));
        }

        return new Tuple<bool, string>(false, null);
    }

    /// <inheritdoc />
    public async Task<bool> LockReleaseAsync(string key, string lockId, CancellationToken token = default)
    {
        try
        {
            bool @lock = await Store.LockReleaseAsync(key, lockId);

            return @lock;
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while release lock key {key} with lock value {lockId}.",
                LogHelpers.Arguments(key, lockId));
        }

        return false;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string key,
        ICacheTransaction transaction = null,
        CancellationToken token = default)
    {
        try
        {
            if (transaction is RedisTransaction { Payload: ITransaction tObj } redisTransaction)
            {
                redisTransaction.Operations.Add(tObj.KeyDeleteAsync(key));

                Logger.LogDebugMessage(
                    "Deleting object with key {key} from temp object store.",
                    LogHelpers.Arguments(key));

                return;
            }

            await Store.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(ex, "An error occurred while deleting key {key}.", LogHelpers.Arguments(key));
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        IEnumerable<string> keys,
        CancellationToken token = default)
    {
        List<string> keysToBeDeleted = keys?
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();

        if (keysToBeDeleted == null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        if (keysToBeDeleted.Count == 0)
        {
            throw new ArgumentException("Sequence cannot be empty.", nameof(keys));
        }

        try
        {
            ITransaction transaction = Store.CreateTransaction();
            var deletionTasks = new List<Task>();

            foreach (string key in keysToBeDeleted)
            {
                deletionTasks.Add(transaction.KeyDeleteAsync(key));

                Logger.LogDebugMessage(
                    "Deleting object with key {key} from temp object store.",
                    LogHelpers.Arguments(key));
            }

            await transaction.ExecuteAsync();
            await Task.WhenAll(deletionTasks);
        }
        catch (Exception ex) when (ex is RedisException || ex is TimeoutException)
        {
            Logger.LogErrorMessage(
                ex,
                "An error occurred while deleting keys {deletedKeys}",
                LogHelpers.Arguments(string.Join("','", keysToBeDeleted)));
        }
    }
}
