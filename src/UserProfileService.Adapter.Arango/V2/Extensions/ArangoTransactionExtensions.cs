using System;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

public static class ArangoTransactionExtensions
{
    private static async Task<TResult> WithLock<TResult>(
        SemaphoreSlim lockObject,
        Func<Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        await lockObject.WaitAsync(cancellationToken);

        try
        {
            return await action.Invoke();
        }
        finally
        {
            lockObject.Release();
        }
    }

    private static async Task WithLock(
        SemaphoreSlim lockObject,
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        await lockObject.WaitAsync(cancellationToken);

        try
        {
            await action.Invoke();
        }
        finally
        {
            lockObject.Release();
        }
    }

    /// <summary>
    ///     Executes the function given in the lock context of the Arango transaction.
    ///     If the transaction is null, no lock context is considered.
    /// </summary>
    /// <typeparam name="TResult">The result type of the task.</typeparam>
    /// <param name="transaction">The <see cref="ArangoTransaction" /> from which the lock object should be used.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation wrapping the result of
    ///     <paramref name="action" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the given action is null.</exception>
    public static Task<TResult> ExecuteWithLock<TResult>(
        this ArangoTransaction transaction,
        Func<Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return transaction == null
            ? action.Invoke()
            : WithLock(transaction.TransactionLock, action, cancellationToken);
    }

    /// <summary>
    ///     Executes the function given in the lock context of the Arango transaction.
    ///     If the transaction is null, no lock context is considered.
    /// </summary>
    /// <param name="transaction">The <see cref="ArangoTransaction" /> from which the lock object should be used.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the given action is null.</exception>
    public static Task ExecuteWithLock(
        this ArangoTransaction transaction,
        Func<Task> action,
        CancellationToken cancellationToken)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return transaction == null
            ? action.Invoke()
            : WithLock(transaction.TransactionLock, action, cancellationToken);
    }
}
