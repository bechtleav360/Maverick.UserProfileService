using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Redis;

/// <summary>
///     Implementation of <see cref="ICacheTransaction" /> that works with Redis implementations.
/// </summary>
public class RedisTransaction : ICacheTransaction
{
    private readonly ITransaction _transaction;

    /// <summary>
    ///     Gets the active operations during this transaction.
    /// </summary>
    public List<Task> Operations { get; } = new List<Task>();

    /// <inheritdoc cref="ICacheTransaction" />
    public object Payload => _transaction;

    /// <summary>
    ///     Initiates a new instance of <see cref="RedisTransaction" />.
    /// </summary>
    /// <param name="transaction"></param>
    public RedisTransaction(ITransaction transaction)
    {
        _transaction = transaction;
    }

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync" />
    public async ValueTask DisposeAsync()
    {
        if (_transaction == null)
        {
            return;
        }

        await Task.WhenAll(Operations);
    }

    /// <inheritdoc cref="ICacheTransaction" />
    public async Task CommitAsync(CancellationToken token = default)
    {
        await _transaction.ExecuteAsync();
        await Task.WhenAll(Operations);
    }

    /// <inheritdoc cref="IDisposable" />
    public void Dispose()
    {
    }
}
