using System;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Abstractions;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace UserProfileService.Common.V2.Implementations;

/// <summary>
///     Represents a cache transaction, that will do nothing and deals as a placeholder for cache implementations that does
///     not support transactions.
/// </summary>
public class DeactivatedCacheTransaction : ICacheTransaction
{
    /// <inheritdoc cref="ICacheTransaction" />
    public object Payload { get; } = new object();

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync" />
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc cref="ICacheTransaction" />
    public Task CommitAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IDisposable" />
    public void Dispose()
    {
    }
}
