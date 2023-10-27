using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Interface to describe a temporary store like redis.
/// </summary>
public interface ITempStore : ICacheStore
{
    /// <summary>
    ///     Get the values of key. If the key does not exist an empty list is returned.
    /// </summary>
    /// <typeparam name="T">Type of values to be returned.</typeparam>
    /// <param name="key">The key of values to be returned.</param>
    /// <param name="end"></param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <param name="start"></param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps the values of a key, or an empty list when
    ///     the key does not exist.
    /// </returns>
    Task<IList<T>> GetListAsync<T>(
        string key,
        long? start = null,
        long? end = null,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default);

    /// <summary>
    ///     Get the length of the list stored at key. If the key does not exist zero is returned.
    /// </summary>
    /// <typeparam name="T">Type of length of list to be returned.</typeparam>
    /// <param name="key">The key of list length to be returned.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps the length of a key, or an zero when the key
    ///     does not exist.
    /// </returns>
    Task<long> GetListLengthAsync<T>(
        string key,
        CancellationToken token = default);

    /// <summary>
    ///     Get a collection of values for the given keys. If the key does not exist the special value is not represented in
    ///     the collection.
    /// </summary>
    /// <typeparam name="T">Type of values to be returned.</typeparam>
    /// <param name="keys">The collection of keys of values to be returned.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a collection of values for the given keys,
    ///     or a empty collection when the keys do not exist.
    /// </returns>
    public Task<IList<T>> GetAsync<T>(
        ISet<string> keys,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default);

    /// <summary>
    ///     Increments the number stored at key by increment. If the key does not exist, it is set to 0 before performing the
    ///     operation.
    /// </summary>
    /// <param name="key">The key of the value to increment,</param>
    /// <param name="value">The amount to increment by (defaults to 1).</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous write operation. It wraps the value at field after the increment operation.
    ///     Null if an error occurred.
    /// </returns>
    public Task<long?> IncrementAsync(string key, int value = 1, CancellationToken token = default);

    /// <summary>
    ///     Add the given object to the list. If no list exists a new one will be created.
    /// </summary>
    /// <typeparam name="T">Type of object to add.</typeparam>
    /// <param name="key">The key of list to add the value.</param>
    /// <param name="add">Value to add to the list.</param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <returns>
    ///     A task that represents the asynchronous write operation. It wraps the number of items in the list after the value
    ///     has been added.
    ///     Null if an error occurred.
    /// </returns>
    public Task<long?> AddAsync<T>(
        string key,
        T add,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default);

    /// <summary>
    ///     Add the given sequence of objects to the list. If no list exists a new one will be created.
    /// </summary>
    /// <typeparam name="T">Type of each element in the sequence.</typeparam>
    /// <param name="key">The key of list to add the value.</param>
    /// <param name="add">Sequence of objects to add to the list.</param>
    /// <param name="expirationTime"></param>
    /// <param name="transaction">
    ///     The transaction object to pass information for a transaction/batch support, if supported by
    ///     this implementation.
    /// </param>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <param name="jsonSettingsProvider">A provider that contains json serializer settings.</param>
    /// <returns>
    ///     A task that represents the asynchronous write operation. It wraps the number of items in the list after the value
    ///     has been added.
    ///     Null if an error occurred.
    /// </returns>
    public Task<long?> AddListAsync<T>(
        string key,
        IEnumerable<T> add,
        int expirationTime = 0,
        ICacheTransaction transaction = default,
        IJsonSerializerSettingsProvider jsonSettingsProvider = null,
        CancellationToken token = default);

    /// <summary>
    ///     Allows creation of a group of operations that will be sent to the server as a single unit,
    ///     and processed on the server as a single unit.
    /// </summary>
    /// <param name="token">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that represents the asynchronous write operation. It wraps the created transaction.</returns>
    public Task<ICacheTransaction> CreateTransactionAsync(CancellationToken token = default);
}
