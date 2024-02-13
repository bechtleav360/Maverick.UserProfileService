using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     A repository for a second level projection which contains read and write operations required for calculating user
///     data.
/// </summary>
public interface ISecondLevelVolatileDataRepository : IProjectionStateRepository
{
    /// <summary>
    ///     Saves the user id in the repository.
    /// </summary>
    /// <param name="userId">The id of the user to be stored.</param>
    /// <param name="transaction">Optional object including information about the transaction to commit.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task SaveUserIdAsync(
        string userId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempt to deletes the user id in the repository.
    /// </summary>
    /// <param name="userId">The id of the user to be deleted.</param>
    /// <param name="transaction">Optional object including information about the transaction to commit.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task TryDeleteUserAsync(
        string userId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to commit.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Aborts an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a transaction and returns an object containing information about the new created transaction.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous operation that wraps the <see cref="IDatabaseTransaction" />.</returns>
    Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default);
}
