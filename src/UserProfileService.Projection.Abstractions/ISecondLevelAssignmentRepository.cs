using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     A repository for a second level projection which contains read and write operations required for calculating all
///     active assignments.
/// </summary>
public interface ISecondLevelAssignmentRepository : IProjectionStateRepository
{
    /// <summary>
    ///     Gets the current assignment user.
    /// </summary>
    /// <param name="userId">
    ///     The id of the user of whom to fetch the assignments from.
    /// </param>
    /// <param name="transaction">
    ///     If set to null no transaction will be used. Otherwise it
    ///     specifies the transaction which should be use for database operations.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represent the asynchronous read operation and
    ///     wraps the return value.
    /// </returns>
    Task<SecondLevelProjectionAssignmentsUser> GetAssignmentUserAsync(
        string userId,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves the specified user. If the user already exists it will be overwritten.
    /// </summary>
    /// <param name="user">The user to save.</param>
    /// <param name="transaction">
    ///     If set to null no transaction will be used. Otherwise it
    ///     specifies the transaction which should be use for database operations.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represent the asynchronous write operation.
    /// </returns>
    Task SaveAssignmentUserAsync(
        SecondLevelProjectionAssignmentsUser user,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the specified user.
    /// </summary>
    /// <param name="userId">The id of the user to delete.</param>
    /// <param name="transaction">
    ///     If set to null no transaction will be used. Otherwise it
    ///     specifies the transaction which should be use for database operations.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represent the asynchronous write operation.
    /// </returns>
    Task RemoveAssignmentUserAsync(
        string userId,
        IDatabaseTransaction transaction = null,
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
}
