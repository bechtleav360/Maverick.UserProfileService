using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Implementations;

/// <summary>
///     A implementation of <see cref="ICacheStore" /> that does nothing and deals as a default implementation.
/// </summary>
public class NoCacheImplementation : ICacheStore
{
    /// <inheritdoc cref="ICacheStore" />
    public Task SetAsync<T>(
        string key,
        T obj,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task<T> GetAsync<T>(
        string key,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        return Task.FromResult<T>(default);
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task<Tuple<bool, string>> LockAsync(
        string key,
        int expirationTime = 0,
        CancellationToken token = default)
    {
        return Task.FromResult(new Tuple<bool, string>(true, default));
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task<bool> LockReleaseAsync(string key, string lockId, CancellationToken token = default)
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task DeleteAsync(IEnumerable<string> keys, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(string key, ICacheTransaction transaction = null, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ICacheStore" />
    public Task DeleteAsync(
        string key,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSerializerSettingsProvider = null,
        CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}
