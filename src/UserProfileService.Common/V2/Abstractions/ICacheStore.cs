using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Provides asnychronous methods to get and set values in a synchronized fashion using locks.
/// </summary>
public interface ICacheStore
{
    /// <summary>
    ///     Set key to hold the value. If key already holds a value, it is overwritten, regardless of its type.
    /// </summary>
    /// <typeparam name="T">Type of value to be set.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="obj">The value to be stored.</param>
    /// <param name="expirationTime">
    ///     Default expiration time after the stored value in store expires and is deleted (in
    ///     seconds).
    /// </param>
    /// <param name="transaction">
    ///     The transaction object to pass information for a transaction/batch support, if supported by
    ///     this implementation.
    /// </param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task SetAsync<T>(
        string key,
        T obj,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSettingsProvider = default,
        CancellationToken token = default);

    /// <summary>
    ///     Get the value of key. If the key does not exist the special value null is returned.
    /// </summary>
    /// <typeparam name="T">Type of value to be returned.</typeparam>
    /// <param name="key">The key of value to be returned.</param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps the value of a key, or null when the key does
    ///     not exist.
    /// </returns>
    Task<T> GetAsync<T>(
        string key,
        IJsonSerializerSettingsProvider jsonSettingsProvider = default,
        CancellationToken token = default);

    /// <summary>
    ///     Takes a lock (specifying a token value) if it is not already taken
    /// </summary>
    /// <param name="key">The key of the lock.</param>
    /// <param name="expirationTime">The expiration of the lock key.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a tuple with the information if the lock was
    ///     successful
    ///     and the corresponding lock value.
    /// </returns>
    public Task<Tuple<bool, string>> LockAsync(
        string key,
        int expirationTime = 0,
        CancellationToken token = default);

    /// <summary>
    ///     Releases a lock, if the lock id is correct.
    /// </summary>
    /// <param name="key">The key of the lock.</param>
    /// <param name="lockId">The lock id at the key that must match.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps true if the lock was successfully released,
    ///     false otherwise.
    /// </returns>
    public Task<bool> LockReleaseAsync(string key, string lockId, CancellationToken token = default);

    /// <summary>
    ///     Deletes the object with specified <paramref name="key" />.
    /// </summary>
    /// <param name="key">The key of the object to be deleted.</param>
    /// <param name="transaction">
    ///     The transaction object to pass information for a transaction/batch support, if supported by
    ///     this implementation.
    /// </param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task DeleteAsync(
        string key,
        ICacheTransaction transaction = default,
        CancellationToken token = default);

    /// <summary>
    ///     Deletes objects with specified <paramref name="keys" />.
    /// </summary>
    /// <param name="keys">The keys of all objects to be deleted.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task DeleteAsync(
        IEnumerable<string> keys,
        CancellationToken token = default);
}
